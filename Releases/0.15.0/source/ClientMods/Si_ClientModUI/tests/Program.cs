using System.IO.Compression;
using System.Globalization;
using System.Text;
using System.Text.Json;
using SiSquadHud;

int checks=0;
void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
void Reject(Action action,string message){bool failed=false;try{action();}catch(ArgumentException){failed=true;}catch(InvalidDataException){failed=true;}Check(failed,message);}
string nonce=Protocol.NewNonce();
Check(Protocol.ValidNonce(nonce),"Nonce");
Check(!Protocol.ValidNonce(nonce.ToUpperInvariant())&&!Protocol.ValidNonce("x"),"Invalid nonce");
Check(!Protocol.ValidId("../escape")&&!Protocol.ValidId("a|b"),"Invalid IDs");
Check(Protocol.ValidId("Si_Mod-v1.2"),"Module ID");
Check(!Protocol.ValidId("界mod"),"Hello IDs stay ASCII to keep the native packet bounded");
foreach(string culture in new[]{"en-US","nl-BE","fr-BE"})
{
    CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
    string h=Protocol.Hello(nonce,0,"abcdef123456",new[]{new KeyValuePair<string,string>("mod","abcdef123456")});
    Check(Protocol.TryHello(h,out var n,out var ch,out var hash,out var known)&&n==nonce&&ch==0&&hash=="abcdef123456"&&known["mod"]==hash,"Hello round trip");
    foreach(string text in new[]{"!kick user","rtv","/1",".back","/say "+new string('界',150),"/buy "})
    {
        Check(Protocol.TryCommand(Protocol.Command(nonce,123,text),out n,out long seq,out string decoded)&&n==nonce&&seq==123&&decoded==text,"Command exact UTF-8 round trip");
    }
}
foreach(string text in new[]{""," ","/x\n/y",new string('a',161),"/test\0"})Reject(()=>Protocol.Command(nonce,1,text),"Reject invalid command");
Check(!Protocol.TryCommand("C|"+nonce+"|-1|L3g=",out _,out _,out _),"Negative sequence");
Check(!Protocol.TryCommand("C|"+nonce+"|1|/w==",out _,out _,out _),"Invalid UTF-8");
Check(!Protocol.TryCommand("C|"+nonce+"|1|?",out _,out _,out _),"Invalid base64");
Check(!Protocol.TryHello("H|"+nonce+"|256||",out _,out _,out _,out _),"Invalid channel");
Check(!Protocol.TryHello("H|"+nonce+"|0||../x:abcdef123456",out _,out _,out _,out _),"Hello ID validation");
string oversized=Protocol.Hello(nonce,0,"abcdef123456",Enumerable.Range(0,120).Select(i=>new KeyValuePair<string,string>("mod"+i,"abcdef123456")));
Check(Protocol.TryHello(oversized,out _,out _,out var empty,out var noKnown)&&empty==""&&noKnown.Count==0,"Oversized hello falls back to full frame");
foreach(string text in new[]{"{\"k\":\"frame\"}",new string('界',30000),string.Join("",Enumerable.Range(0,5000).Select(_=>Guid.NewGuid().ToString()))})
{
    var packets=Protocol.Transfer(nonce,text);var chunks=new List<Chunk>();
    foreach(string packet in packets){Check(packet.Length<=Protocol.MaxBody&&Protocol.TryTransfer(packet,out _),"Chunk bounded");Protocol.TryTransfer(packet,out var chunk);chunks.Add(chunk);}
    var reordered=chunks.OrderByDescending(c=>c.Index).ToDictionary(c=>c.Index,c=>c.Bytes);
    Check(Protocol.Assemble(Enumerable.Range(0,chunks.Count).Select(i=>reordered[i]).ToList(),chunks[0].Gzip)==text,"Raw/gzip multichunk reassembly");
}
Check(!Protocol.TryTransfer("T|"+nonce+"|"+nonce+"|2|2|r|AA==",out _),"Invalid chunk index");
Check(!Protocol.TryTransfer("T|"+nonce+"|"+nonce+"|0|513|r|AA==",out _),"Chunk count bound");
Check(!Protocol.TryTransfer("T|"+nonce+"|"+nonce+"|0|1|q|AA==",out _),"Unknown compression");
Reject(()=>Protocol.Transfer(nonce,new string('x',Protocol.MaxDocumentBytes+1)),"Outbound document cap");
Reject(()=>Protocol.Assemble(new[]{new byte[Protocol.ChunkBytes+1]},false),"Part size cap");
using(var compressed=new MemoryStream())
{
    using(var zip=new GZipStream(compressed,CompressionLevel.Optimal,true))zip.Write(new byte[Protocol.MaxDocumentBytes+1]);
    byte[] bytes=compressed.ToArray();var parts=Enumerable.Range(0,(bytes.Length+Protocol.ChunkBytes-1)/Protocol.ChunkBytes).Select(i=>bytes.Skip(i*Protocol.ChunkBytes).Take(Protocol.ChunkBytes).ToArray()).ToArray();
    Reject(()=>Protocol.Assemble(parts,true),"Gzip expansion cap");
}
ServerModule Module(string id,string text,bool admin=false)=>new(){Id=id,Name=id,Hash=Protocol.ShortHash(text),Commands=new(){new(){Text=text,Label=text,Admin=admin}}};
ServerFrame Frame(params ServerModule[] mods)=>new(){Full=true,Modules=mods.ToList(),Root=Protocol.ShortHash(string.Join("\n",mods.OrderBy(m=>m.Id,StringComparer.Ordinal).Select(m=>m.Id+":"+m.Hash)))};
var catalog=new CompanionCatalog();var a=Module("A","/buy");var b=Module("B","!kick",true);
var first=Frame(a,b);catalog.Apply(first);Check(catalog.Modules.Count==2&&catalog.Modules["B"].Commands[0].Admin,"Full catalog");
catalog.Apply(new(){Root=first.Root,Same=true});Check(catalog.Root==first.Root,"Unchanged confirmation");
var a2=Module("A","/buy changed");var next=Frame(a2);next.Full=false;next.Removed.Add("B");catalog.Apply(next);
Check(catalog.Modules.Count==1&&catalog.Modules["A"].Commands[0].Text=="/buy changed","Atomic permission removal and diff merge");
string snapshot=catalog.Snapshot();var restored=new CompanionCatalog();restored.Apply(JsonSerializer.Deserialize<ServerFrame>(snapshot));Check(restored.Root==catalog.Root&&restored.Modules.Count==1,"Disk cache round trip");
Reject(()=>catalog.Apply(new(){Root=first.Root,Same=true}),"Unknown cached root");
Reject(()=>catalog.Apply(new(){Root=first.Root,Full=true,Modules=new(){a}}),"Missing diff content");
Check(catalog.Snapshot()==snapshot,"Rejected diff does not mutate active catalog");
var bad=Frame(a2);bad.Modules[0].Navigation="/x|";Reject(()=>catalog.Apply(bad),"Invalid menu prefix");
bad=Frame(Module("A","/buy\n!kick"));Reject(()=>catalog.Apply(bad),"Injected command in frame");
bad=Frame(Module("../path","/x"));Reject(()=>catalog.Apply(bad),"Unsafe module ID");
catalog.Clear();Check(catalog.Root==""&&catalog.Modules.Count==0,"Disconnect clears catalog");
SquadHud.RunNetworkChecks(Check);
Console.WriteLine($"PASS: {checks} network, catalog and client lifecycle checks. No game/server started.");

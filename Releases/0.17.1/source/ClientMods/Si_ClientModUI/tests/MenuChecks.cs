namespace SiSquadHud;

internal static class MenuChecks
{
    internal static void Run(Action<bool,string> check)
    {
        var buy=ServerMenuParser.Parse(new[]{"[Buy] === Buy Menu === Credits: 2000","  1. Vehicles (12)","  2. Infantry (20)","/1-/N drill in · /0 to close  (or /buy <N>)"},"/",new[]{"/buy"});
        check(buy.Count==3&&buy[0].Command=="/1"&&buy[0].Label=="Vehicles (12)"&&buy[2].Command=="/0","Buy rows and actual close footer");
        check(!buy.Any(c=>c.Command=="/back"),"No invented back at root");
        var formation=ServerMenuParser.Parse(new[]{"Formations: Formation functions\n/1 Move\n/2 Follow\n/3 Attack\n/4 Disband all\n/5 Lock formations (currently unlocked)"},"/",new[]{"/formation"});
        check(formation.Count==5&&formation[0].Label=="Move"&&formation[4].Command=="/5","Formation multiline explicit commands");
        var shapes=ServerMenuParser.Parse(new[]{"Formations: move — Current formation: Default","Formations: /1 Default","Formations: /2 Line","Formations: /10 Wedge","Formations: /back returns; /formation closes"},"/",new[]{"/formation"});
        check(shapes.Count==5&&shapes[0].Command=="/1"&&shapes[2].Command=="/10","Formation prefixed rows retain server ordering");
        check(shapes.Any(c=>c.Command=="/formation"&&c.Label=="Close menu")&&!shapes.Any(c=>c.Command=="/0"),"Formation advertises its actual toggle-close command");
        var balance=ServerMenuParser.Parse(new[]{"<b><color=#DDE98C>[</color><color=#7DD4FF>BAL</color><color=#DDE98C>]</color></b> <color=#AAFFAA>1.</color> Sol  <color=#AAFFAA>2.</color> Centauri  <color=#AAFFAA>3.</color> Alien  4. JSON  5. HTP  6. Cap&Decay  7. Strategystyle"},"/",new[]{"!b"});
        check(balance.Count==7&&balance[1].Command=="/2"&&balance[1].Label=="Centauri"&&balance[6].Label=="Strategystyle","UnitBalance colored prefixed inline choices");
        var values=ServerMenuParser.Parse(new[]{"[BAL] 1. health_mult = 1.5 (base: 100)","[BAL] 2. damage_mult = 0.25 (base: 10)","[BAL] Set: .1 1.5 · toggle 1=ON/0=OFF"},"/",new[]{"!b"});
        check(values.Count==2&&values[0].Label.Contains("1.5")&&values[1].Label.Contains("0.25"),"Value and baseline decimals are preserved as labels");
        var explicitPrefix=ServerMenuParser.Parse(new[]{"[Editor] .1 Health  .2 Damage","/back to return; .0 closes"},"!",Array.Empty<string>());
        check(explicitPrefix.Count==4&&explicitPrefix[0].Command==".1"&&explicitPrefix[2].Command=="/back"&&explicitPrefix[3].Command==".0","Explicit command prefixes override default without guessing");
        var falseRows=ServerMenuParser.Parse(new[]{"Health: 100.5","Version 1.2.3","/1-/N to select","Set: .1 1.5","Credits: 2000","No menu output."},"/",Array.Empty<string>());
        check(falseRows.Count==0,"Stats, versions, menu range syntax and value examples are not choices");
        var duplicate=ServerMenuParser.Parse(new[]{"[Mod] 1. First","[Mod] 1. First","/back and /back; /0 close"},"/",Array.Empty<string>());
        check(duplicate.Count==3,"Duplicate options and footers deduplicate");
        var unknown=ServerMenuParser.Parse(new[]{"/unknown closes"},"/",new[]{"/known"});
        check(unknown.Count==0,"Toggle-close commands require confirmed registration");
        check(ServerMenuParser.Parse(new[]{"1. Unsafe"},"invalid",Array.Empty<string>()).Count==0,"Invalid default prefix rejected");
        var limits=ServerMenuParser.Parse(Enumerable.Range(1,1000).Select(i=>i+". Choice"),"/",Array.Empty<string>());
        check(limits.Count==200,"Bounded reply line count");
    }
}

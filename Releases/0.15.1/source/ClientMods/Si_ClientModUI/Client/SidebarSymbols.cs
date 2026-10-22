using UnityEngine;
using MelonLoader;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private sealed class SymbolChoice {internal string Id,Label,Glyph;internal Texture2D Texture;}
    private readonly Dictionary<string,(Texture2D texture,GUIStyle style)> symbolTextures=new();
    private readonly Dictionary<string,SymbolChoice> symbolLookup=new();
    private SymbolChoice[] symbolCatalogCache=Array.Empty<SymbolChoice>();
    private double nextSymbols=-1;
    private GUIStyle symbolGlyphStyle;
    private IEnumerable<SymbolChoice> SymbolCatalog()
    {
        if(clock.Elapsed.TotalSeconds>=nextSymbols)
        {
            nextSymbols=clock.Elapsed.TotalSeconds+1;symbolCatalogCache=BuildSymbolCatalog().ToArray();symbolLookup.Clear();
            foreach(var choice in symbolCatalogCache)symbolLookup[choice.Id]=choice;
        }
        return symbolCatalogCache;
    }
    private IEnumerable<SymbolChoice> BuildSymbolCatalog()
    {
        foreach(char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789$+-!?%&=@")
            yield return new SymbolChoice{Id="text:"+c,Label=c.ToString(),Glyph=c.ToString()};
        foreach(string shape in new[]{"circle","square","triangle","diamond","hexagon","star"})
            foreach(string fill in new[]{"filled","open"})yield return new SymbolChoice{Id="shape:"+shape+"-"+fill,Label=shape+" "+fill};
        foreach(string id in new[]{"padlock","line","plus","check","cross","smile","frown","heart","lightning","sun"})
            yield return new SymbolChoice{Id="icon:"+id,Label=id};
        foreach(var owner in localMods)
        {
            var symbols=ClientModControls.For(owner)?.SidebarSymbols;if(symbols==null)continue;
            foreach(var symbol in symbols)if(symbol!=null&&symbol.Texture!=null&&!string.IsNullOrWhiteSpace(symbol.Id))
                yield return new SymbolChoice{Id=LocalScope(owner)+":symbol:"+symbol.Id,Label=LocalTitle(owner)+" / "+symbol.Label,Texture=symbol.Texture};
        }
    }
    private void DrawSymbol(Rect rect,string id,Color color)
    {
        SymbolCatalog();symbolLookup.TryGetValue(id??"",out var symbol);
        if(symbolGlyphStyle==null){symbolGlyphStyle=Text(26);symbolGlyphStyle.alignment=TextAnchor.MiddleCenter;}
        var previous=GUI.color;
        try
        {
            GUI.color=color;
            if(symbol?.Glyph!=null){GUI.Label(rect,symbol.Glyph,symbolGlyphStyle);return;}
            if(symbol?.Texture!=null){var style=new GUIStyle();style.normal.background=symbol.Texture;GUI.Box(rect,"",style);return;}
            if(symbol==null){GUI.Label(rect,"?",symbolGlyphStyle);return;}
            if(!symbolTextures.TryGetValue(id,out var cached))
            {
                var texture=new Texture2D(48,48,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp};
                for(int y=0;y<48;y++)for(int x=0;x<48;x++)
                    texture.SetPixel(x,y,new Color(1,1,1,SymbolInk(id,(x-23.5f)/24,(y-23.5f)/24)?1:0));
                texture.Apply();var style=new GUIStyle();style.normal.background=texture;
                cached=(texture,style);symbolTextures[id]=cached;
            }
            GUI.Box(rect,"",cached.style);
        }
        finally{GUI.color=previous;}
    }
    private static float SegmentDistance(float x,float y,float ax,float ay,float bx,float by)
    {
        float dx=bx-ax,dy=by-ay,t=Math.Clamp(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy),0,1);
        return MathF.Sqrt((x-ax-t*dx)*(x-ax-t*dx)+(y-ay-t*dy)*(y-ay-t*dy));
    }
    private static bool PolygonInk(float x,float y,(float x,float y)[] points,bool open)
    {
        bool inside=false;float edge=100;
        for(int i=0,j=points.Length-1;i<points.Length;j=i++)
        {
            var a=points[i];var b=points[j];
            if((a.y>y)!=(b.y>y)&&x<(b.x-a.x)*(y-a.y)/(b.y-a.y)+a.x)inside=!inside;
            edge=Math.Min(edge,SegmentDistance(x,y,a.x,a.y,b.x,b.y));
        }
        return open?edge<.065f:inside;
    }
    private static bool SymbolInk(string id,float x,float y)
    {
        float ax=Math.Abs(x),ay=Math.Abs(y),r=MathF.Sqrt(x*x+y*y);bool open=id.EndsWith("-open");
        string shape=id.Replace("shape:","").Replace("-filled","").Replace("-open","").Replace("icon:","");
        switch(shape)
        {
            case "circle":return open?Math.Abs(r-.72f)<.065f:r<.72f;
            case "square":return Math.Max(ax,ay)<.72f&&(!open||Math.Max(ax,ay)>.59f);
            case "diamond":return ax+ay<.94f&&(!open||ax+ay>.78f);
            case "triangle":return PolygonInk(x,y,new[]{(0f,.8f),(-.78f,-.66f),(.78f,-.66f)},open);
            case "hexagon":float edge=Math.Max(ax*.8660254f+ay*.5f,ay);return edge<.72f&&(!open||edge>.59f);
            case "star":
                var points=new (float,float)[10];
                for(int i=0;i<10;i++){float angle=MathF.PI/2+i*MathF.PI/5,size=i%2==0?.85f:.36f;points[i]=(MathF.Cos(angle)*size,MathF.Sin(angle)*size);}
                return PolygonInk(x,y,points,open);
            case "padlock":return (ax<.5f&&y>-.7f&&y<.08f&&!(ax<.07f&&y>-.48f&&y<-.13f))||
                (y>=.02f&&Math.Abs(MathF.Sqrt(x*x+(y-.12f)*(y-.12f))-.4f)<.075f);
            case "line":return ax<.78f&&ay<.07f;
            case "plus":return ax<.7f&&ay<.075f||ay<.7f&&ax<.075f;
            case "cross":return ax<.66f&&ay<.66f&&(Math.Abs(x-y)<.11f||Math.Abs(x+y)<.11f);
            case "check":return SegmentDistance(x,y,-.7f,-.04f,-.18f,-.48f)<.095f||SegmentDistance(x,y,-.18f,-.48f,.7f,.65f)<.095f;
            case "heart":float hx=x/0.66f,hy=y/0.66f,a=hx*hx+hy*hy-1;return a*a*a-hx*hx*hy*hy*hy<0&&r<.97f;
            case "lightning":return PolygonInk(x,y,new[]{(.2f,.86f),(-.57f,-.12f),(-.08f,-.12f),(-.22f,-.9f),(.62f,.24f),(.1f,.24f)},false);
            case "sun":return r<.35f||r>.5f&&r<.86f&&Math.Abs(MathF.Sin(MathF.Atan2(y,x)*4))<.22f;
            case "smile":
            case "frown":
                bool eyes=(x-.27f)*(x-.27f)+(y-.23f)*(y-.23f)<.014f||(x+.27f)*(x+.27f)+(y-.23f)*(y-.23f)<.014f;
                bool mouth=shape=="smile"?y<-.12f&&Math.Abs(MathF.Sqrt(x*x+(y-.04f)*(y-.04f))-.46f)<.065f:
                    y>-.43f&&y<-.12f&&Math.Abs(MathF.Sqrt(x*x+(y+.62f)*(y+.62f))-.42f)<.065f;
                return Math.Abs(r-.78f)<.065f||eyes||mouth;
            default:return false;
        }
    }
    private static Color ReadColor(string text)
    {
        text=(text??"").TrimStart('#');
        if(text.Length!=6||!int.TryParse(text,System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture,out int n))return Color.white;
        return new Color(((n>>16)&255)/255f,((n>>8)&255)/255f,(n&255)/255f,1);
    }
    private void DisposeSymbols()
    {foreach(var pair in symbolTextures.Values)if(pair.texture!=null)UnityEngine.Object.Destroy(pair.texture);symbolTextures.Clear();}
}


using System.Text.Json.Serialization;
namespace SiSquadHud;
internal sealed class ControlPresentation
{
    [JsonPropertyName("kind")] public string Kind {get;set;}="";
    [JsonPropertyName("group")] public string Group {get;set;}="";
    [JsonPropertyName("min")] public double? Min {get;set;}
    [JsonPropertyName("max")] public double? Max {get;set;}
    [JsonPropertyName("step")] public double? Step {get;set;}
    [JsonPropertyName("unit")] public string Unit {get;set;}="";
    [JsonPropertyName("default")] public string Default {get;set;}
    [JsonPropertyName("vanilla")] public string Vanilla {get;set;}
    public bool Valid()=>Kind!=null&&Kind.Length<=32&&Group!=null&&Group.Length<=160&&Unit!=null&&Unit.Length<=40&&
        Default?.Length>2048!=true&&Vanilla?.Length>2048!=true&&
        (!Min.HasValue||double.IsFinite(Min.Value))&&(!Max.HasValue||double.IsFinite(Max.Value))&&
        (!Step.HasValue||double.IsFinite(Step.Value)&&Step.Value>0)&&
        (!Min.HasValue||!Max.HasValue||Min<=Max)&&
        (Kind!="slider"||(!Min.HasValue||Math.Abs(Min.Value)<=float.MaxValue)&&(!Max.HasValue||Math.Abs(Max.Value)<=float.MaxValue));
}

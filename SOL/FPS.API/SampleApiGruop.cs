using Template.API.Attributes;

namespace Template.API;

public enum SampleApiGroup
{
    [DocInfoGenerator(title: "All APIs")]
    All,

    [DocInfoGenerator(title: "Dashboard APIs")]
    Dashboard,

    [DocInfoGenerator(title: "Mobile Dashboard APIs")]
    MobileDashboard,
}
namespace Northwind.Mvc.Models;

public class WebsiteContext
{
    public Version Version { get; set; }
    public int CopyrightYear { get; set; }
    public bool Approved { get; set; }
    public int TagsToShow { get; set; }
    
}
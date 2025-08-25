namespace GymAdmin.Desktop.Models;

public class NavigationItem
{
    public string Name { get; set; }
    public string ViewName { get; set; }

    public NavigationItem(string name, string viewName)
    {
        Name = name;
        ViewName = viewName;
    }
}
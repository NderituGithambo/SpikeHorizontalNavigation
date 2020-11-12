# SpikeHorizontalNavigation
Horizontal Navigation for Devexpress xaf using Classic themes

This a simple Devexpress xaf web that illustrates how you would build an app with a horizontal navigation using the deprecated Classic themes and using **C#**.

## Steps
1. In your **Web project**, add a new **devexpress item** and choose **Default Template** 
2. Add the two files in Repo, namely: [ASPxMenuNavigationContainer.cs](https://github.com/NderituGithambo/SpikeHorizontalNavigation/blob/master/SpikeHorizontalNavigation.Web/ASPxMenuNavigationActionContainer.cs) and [ASPxMenuNavigationControl.cs](https://github.com/NderituGithambo/SpikeHorizontalNavigation/blob/master/SpikeHorizontalNavigation.Web/ASPxMenuNavigationControl.cs)
3. Open your **.ascx** file and in the file name use a custom namespace in the Inherits like **Inherits="Custom.Namespace.Web.HorizontalTemplateContent"**
4. Register your namespaces. That is, Your custom namespace and the namespace for your current project
5. Copy and paste the rest of the markup code from the ascx template file into your own
6. In your **template.ascx.cs** change **namespace** to your **custom one** and include it as part of your directives in the **ASPxMenuNavigationContainer.cs** file

using System.Text;
using HtmlAgilityPack;

namespace JobApplicationTracker;
public class JobPostingService
{
    private static int HttpByteLimit = 10 * 1024 * 1024 + 1;//10 MB download limit
    private static string[] excludedElements =
    {
        "script", "style", "noscript", "template",
        "svg", "canvas", "iframe", "object", "embed",
        "nav", "footer", "form", "button",
        "select", "option", "optgroup",
        "input", "textarea", "label", "dialog",
        "head", "meta", "link", "base", "a", "header"
    };

    // get the readable text from the website at jobLink
    public static async Task<string?> getSearchableText(string jobLink)
    {
        try
        {
            var httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(jobLink);
            response.EnsureSuccessStatusCode();

            using Stream httpStream = await response.Content.ReadAsStreamAsync();
            var buffer = new byte[HttpByteLimit];
            int bytesRead = await httpStream.ReadAsync(buffer, 0, HttpByteLimit);

            if (bytesRead >= HttpByteLimit)
            {
                Console.WriteLine("HTTP response was too large");
                return null;
            }

            string result = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var html = new HtmlDocument();
            html.LoadHtml(result);

            //remove excluded elements from html nodes
            foreach (string element in excludedElements)
            {
                HtmlNodeCollection? nodes = html.DocumentNode.SelectNodes($"//{element}");

                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes.ToList())
                    {
                        node.Remove();
                    }
                }
            }
            
            //of the remaining nodes, extract their text
            var textNodes = new List<string>();
            foreach (HtmlNode node in html.DocumentNode.Descendants())
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    string text = HtmlEntity.DeEntitize(node.InnerText).Trim();
                    if (text != "") 
                    {
                        textNodes.Add(text);
                    }
                }
            }
            return string.Join(" ", textNodes);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Http GET failed with following exception:");
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}

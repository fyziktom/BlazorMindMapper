using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace MindMapper
{
    public class MindMapperService
    {
        private readonly IJSRuntime js;
        private readonly HttpClient httpClient;

        public readonly string MindMapStart = "mindmap";
        public readonly string EndLine = "\r\n";
        public readonly string Tab = "\t";

        public string Name { get; set; } = string.Empty;
        public string MindMapMermaid { get; set; } = string.Empty;

        public TreeItem Root { get; set; } = new TreeItem();
        public MindMapperService(IJSRuntime js, IServiceProvider serviceProvider)
        {
            this.js = js;
            this.httpClient = serviceProvider.GetRequiredService<HttpClient>();
        }

        /// <summary>
        /// Download file with MindMap
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="aslist"></param>
        /// <returns></returns>
        public async Task BackupMindMap()
        {
            var filename = $"Backup-MindMap-{Name}_Time_" + DateTime.UtcNow.ToString("dd-MM-yyyyThh_mm_ss") + ".mmd";
            
            if (!string.IsNullOrEmpty(MindMapMermaid))
            {
                await js.InvokeVoidAsync("mindMapper.downloadText", MindMapMermaid, filename);
            }
        }

        /// <summary>
        /// Initialize Mermaid
        /// </summary>
        /// <returns></returns>
        public async Task InitMermaid()
        {
            await js.InvokeVoidAsync("mindMapper.MermaidInitialize");
            await js.InvokeVoidAsync("mindMapper.MermaidRender");
        }


        public string CleanText(string i)
        {
            return i.Replace("```mermaid\n\n", string.Empty).Replace("```", string.Empty);
        }
        public string MermaidWrappedText(string i)
        {
            if (!i.Contains("```mermaid"))
                return string.Concat("```mermaid\n\n", i, "\n```");
            return i;
        }

        private string GetTabsBasedOnDepth(int depth)
        {
            var result = Tab;
            for (int i = 0; i < depth; i++)
                result += Tab;

            return result;
        }

        public string GetMindMapFromTree(TreeItem? root = null)
        {
            var result = MindMapStart + EndLine;

            Queue<TreeItem> queue = new Queue<TreeItem>();

            if (root == null && Root != null)
                root = Root;
            else if (root != null)
                Root = root;

            if (root != null)
                queue.Enqueue(root);

            //explore the tree and load all childerns
            while (queue.Count > 0)
            {
                var q = queue.Dequeue();
                if (q != null)
                {
                    result += GetTabsBasedOnDepth(q.Depth) + $"{q.Value}" + EndLine;
                    //result += GetTabsBasedOnDepth(q.Depth) + $"{q.Value} <a href=\"https://google.com/\" target=\"_blank\">google</a>" + EndLine;
                    //result += GetTabsBasedOnDepth(q.Depth) + $"click {q.Value} href \"https://google.com\"" + EndLine;
                    if (!string.IsNullOrEmpty(q.Icon))
                        result += GetTabsBasedOnDepth(q.Depth) + q.Icon + EndLine;

                    if (q.Children != null)
                    {
                        foreach (var kid in q.Children)
                            queue.Enqueue(kid);
                    }
                }
            }

            MindMapMermaid = result;

            return result;
        }
    }
}

namespace GenBOE.Common.IO.Export
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using GenBOE.Common.IO.Utilities;

    [System.Diagnostics.DebuggerDisplay("{Tag}, parent = {ParentTag}")]
    public class TagTreeNode
    {
        private ICollection<TagTreeNode> children = new List<TagTreeNode>();

        public TagTreeNode()
        {
        }

        public TagTreeNode(SdtElement element, SdtElement parentElement)
            : base()
        {
            this.Element = element;
            this.ParentElement = parentElement;
        }

        public string Tag { get; set; }

        public string ParentTag { get; set; }
            
        public ICollection<TagTreeNode> Children { get { return this.children; } }

        public bool IsContainer { get; set; }

        public SdtElement Element { get; set; }

        public SdtElement ParentElement { get; set; }
        
        public bool Deleted { get; set; }
    }

    /// <summary>
    /// Method for processing the node
    /// </summary>
    /// <param name="node">Node</param>
    public delegate void ProcessTagTreeNodeDelegate(TagTreeNode node);

    public static class BOEExporterDocumentTree
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static void RunTest(WordprocessingDocument document, object parameterData, StreamWriter log)
        {
            TagTreeNode tree = BuildTaggedElementTree(document);

            Traverse(tree, log);

            ////MyNode wbsNumberNode = GetTaggedTreeNode(tree, "BOEContainer", "BOEHeader", "WBSNumber");
            ////string expectedWbsNumber = wbsNumberNode.Element.InnerText;  // WBS#

            ////MyNode elementOfCostNode = GetTaggedTreeNode(tree, "BOEContainer", "TaskContainer-Labor", "PerformingOrgsTable", "ElementOfCost");
            ////string expectedElementOfCost = elementOfCostNode.Element.InnerText;  // ElemOfCost

            return;
        }

        public static TagTreeNode BuildTaggedElementTree(WordprocessingDocument document)
        {
            IEnumerable<Tag> tags = WordUtilities.GetTags(document);
            return BuildTaggedElementTree(tags);
        }

        public static TagTreeNode BuildTaggedElementTree(OpenXmlElement rootElement)
        {
            IEnumerable<Tag> tags = WordUtilities.GetTags(rootElement);
            return BuildTaggedElementTree(tags);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        public static TagTreeNode BuildTaggedElementTree(IEnumerable<Tag> tags)
        {
            // On first pass, assemble parent and child collections

            ////ICollection<MyTagTreeNodeInit> contentContainers = new List<MyTagTreeNodeInit>();
            ////ICollection<MyTagTreeNodeInit> contentItems = new List<MyTagTreeNodeInit>();
            ICollection<TagTreeNode> contentContainers = new List<TagTreeNode>();
            ICollection<TagTreeNode> contentItems = new List<TagTreeNode>();

            foreach (Tag tag in tags)
            {
                SdtElement element = tag.Ancestors<SdtElement>().FirstOrDefault();

                bool isContentContainer = (element.HasChildren && element.Descendants<SdtElement>().Any());

                SdtElement parentContainerElement = element.Ancestors<SdtElement>().FirstOrDefault(e => e.HasChildren && e.Descendants<SdtElement>().Any());
                Tag parentContainerTag = null;
                string parentContainerTagValue = null;

                if (parentContainerElement != null)
                {
                    parentContainerTag = parentContainerElement.Descendants<Tag>().FirstOrDefault();
                    if (parentContainerTag != null)
                    {
                        parentContainerTagValue = parentContainerTag.Val.HasValue ? parentContainerTag.Val.Value : string.Empty;
                    }
                }

                ////MyTagTreeNodeInit node = new MyTagTreeNodeInit(element, parentContainerElement)
                TagTreeNode node = new TagTreeNode(element, parentContainerElement)
                {
                    Tag = tag.Val.HasValue ? tag.Val.Value : string.Empty,
                    ParentTag = parentContainerTagValue,
                    IsContainer = isContentContainer
                };

                if (isContentContainer)
                {
                    contentContainers.Add(node);
                }
                else
                {
                    contentItems.Add(node);
                }
            }

            // Now assign each child to its parent
            ////foreach (MyTagTreeNodeInit child in contentItems)
            foreach (TagTreeNode child in contentItems)
            {
                ////MyTagTreeNodeInit matchingParent = contentContainers.FirstOrDefault(p => p.Element == child.ParentElement);
                TagTreeNode matchingParent = contentContainers.FirstOrDefault(p => p.Element == child.ParentElement);
                if (matchingParent != null)
                {
                    matchingParent.Children.Add(child);
                    child.Deleted = true;
                }
            }

            ////foreach (MyTagTreeNodeInit parent in contentContainers)
            foreach (TagTreeNode parent in contentContainers)
            {
                ////MyTagTreeNodeInit matchingParent = contentContainers.FirstOrDefault(p => p.Element == parent.ParentElement);
                TagTreeNode matchingParent = contentContainers.FirstOrDefault(p => p.Element == parent.ParentElement);
                if (matchingParent != null)
                {
                    matchingParent.Children.Add(parent);
                    parent.Deleted = true;
                }
            }

            ////MyTagTreeNodeInit rootNode = contentContainers.Where(i => !i.Deleted).FirstOrDefault();
            TagTreeNode rootNode = contentContainers.Where(i => !i.Deleted).FirstOrDefault();

            return rootNode;
        }

        public static void Traverse(TagTreeNode tree, StreamWriter log)
        {
            IEnumerable<TagTreeNode> nodes = new List<TagTreeNode> { tree };
            Traverse(nodes, 1, log);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        private static void Traverse(IEnumerable<TagTreeNode> nodes, int level, StreamWriter log)
        {
            string indent = new string(' ', level * 4);

            foreach (TagTreeNode node in nodes)
            {
                ////log.WriteLine("{0}[{1}]: {2}",
                ////    indent,
                ////    node.Tag,
                ////    node.IsContainer ? string.Empty : node.Element.InnerText);

                log.WriteLine("{0}[{1}]",
                    indent,
                    node.Tag);

                Traverse(node.Children, level + 1, log);
            }
        }

        public static void TraverseDepthFirst(TagTreeNode tree, ProcessTagTreeNodeDelegate process) //, StreamWriter log)
        {
            IEnumerable<TagTreeNode> nodes = new List<TagTreeNode> { tree };
            TraverseDepthFirst(nodes, process); //, log);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        private static void TraverseDepthFirst(IEnumerable<TagTreeNode> nodes, ProcessTagTreeNodeDelegate process) //, StreamWriter log)
        {
            foreach (TagTreeNode node in nodes)
            {
                if (node.Children.Any())
                {
                    TraverseDepthFirst(node.Children, process); //, log);
                }
                else
                {
                    process(node);

                    ////log.WriteLine("{0} [{1}] {2}", node.Tag ?? string.Empty, node.ParentTag ?? string.Empty, node.IsContainer ? "CONTAINER" : string.Empty);
                }
            }
        }

        public static TagTreeNode GetTaggedTreeNode(TagTreeNode tree, params string[] tags)
        {
            return GetTaggedTreeNode(new List<TagTreeNode> { tree }, 0, tags);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        private static TagTreeNode GetTaggedTreeNode(IEnumerable<TagTreeNode> nodes, int tagIdx, params string[] tags)
        {
            TagTreeNode node = null;

            if (tagIdx < tags.Length)
            {
                // TODO:SJR - BOEHeader contains CHILDREN that do match
                TagTreeNode match = nodes.FirstOrDefault(n => n.Tag == tags[tagIdx]);
                if (match != null)
                {
                    if (tagIdx + 1 == tags.Length)
                    {
                        node = match;
                    }
                    else
                    {
                        node = GetTaggedTreeNode(match.Children, tagIdx + 1, tags);
                    }
                }
            }

            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static IDictionary<string, SdtElement> GetContentContainers(WordprocessingDocument document, object parameterData, StreamWriter log)
        {
            IDictionary<string, SdtElement> containers = new Dictionary<string, SdtElement>();

            IEnumerable<Tag> tags = WordUtilities.GetTags(document);

            foreach (Tag tag in tags)
            {
                SdtElement element = tag.Ancestors<SdtElement>().FirstOrDefault();
                bool isContentContainer = (element.HasChildren && element.Descendants<SdtElement>().Any());
                if (isContentContainer)
                {
                    Tag tagObj = tag.NextSibling<Tag>();
                    containers.Add(tagObj.Val.Value, element);
                }
            }

            return containers;
        }
    }
}

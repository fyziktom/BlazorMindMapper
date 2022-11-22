using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMapper
{
    public class TreeItem
    {
        /// <summary>
        /// Id of the Item
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();   
        /// <summary>
        /// Name of the Item
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Depth in the tree
        /// </summary>
        public int Depth { get; set; } = 0;
        /// <summary>
        /// Parent TreeItem
        /// </summary>
        public TreeItem? Parent { get; set; }
        /// <summary>
        /// Collection of TreeItem childs
        /// </summary>
        public ICollection<TreeItem>? Children { get; set; } = new List<TreeItem>();
        /// <summary>
        /// Check if this item is parent = contains some children
        /// </summary>
        public bool IsParent { get => Children?.Count > 0; }
        /// <summary>
        /// Value of Tree Item
        /// </summary>
        public string Value { get; set; } = string.Empty;
        /// <summary>
        /// Icon
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        // Space to skip horizontally between siblings
        // and vertically between generations.
        public static float Hoffset = 10;
        public static float Voffset = 15;

        // The node's center after arranging.
        public PointF Center { get; set; } = new PointF(0, 0);

        /// <summary>
        /// Get Enumerable of all childs
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeItem> GetChilds()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                    yield return child;
            }
        }

        /// <summary>
        /// Add one child to the TreeItem
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddChild(TreeItem item)
        {
            if (Children == null) Children = new List<TreeItem>();

            if (item != null && !Children.Contains(item))
            {
                item.Depth = Depth + 1;
                item.Parent = this;
                Children.Add(item);
                return true;
            }

            return false;
        }

        public TreeItem ContainsChild(string id)
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    if (child.Id == id)
                        return child;
                }
            }
            return null;
        }

        /// <summary>
        /// http://www.csharphelper.com/howtos/howto_random_generic_tree.html
        /// </summary>
        /// <param name="xmin"></param>
        /// <param name="ymin"></param>
        // Arrange the node and its children in the allowed area.
        // Set xmin to indicate the right edge of our subtree.
        // Set ymin to indicate the bottom edge of our subtree.
        public void Arrange(ref float xmin, ref float ymin)
        {
            // See how big this node is.
            SizeF my_size = new SizeF(120, 40);

            // Recursively arrange our children,
            // allowing room for this node.
            float x = xmin;
            float biggest_ymin = ymin + my_size.Height;
            float subtree_ymin = ymin + my_size.Height + Voffset;

            foreach (TreeItem child in Children)
            {
                // Arrange this child's subtree.
                float child_ymin = subtree_ymin;
                child.Arrange(ref x, ref child_ymin);

                // See if this increases the biggest ymin value.
                if (biggest_ymin < child_ymin) biggest_ymin = child_ymin;

                // Allow room before the next sibling.
                x += Hoffset;
            }

            // Remove the spacing after the last child.
            if (Children.Count > 0) x -= Hoffset;

            // See if this node is wider than the subtree under it.
            float subtree_width = x - xmin;
            if (my_size.Width > subtree_width)
            {
                // Center the subtree under this node.
                // Make the children rearrange themselves
                // moved to center their subtrees.
                x = xmin + (my_size.Width - subtree_width) / 2;
                foreach (TreeItem child in Children)
                {
                    // Arrange this child's subtree.
                    child.Arrange(ref x, ref subtree_ymin);

                    // Allow room before the next sibling.
                    x += Hoffset;
                }

                // The subtree's width is this node's width.
                subtree_width = my_size.Width;
            }

            // Set this node's center position.
            Center = new PointF(
                xmin + subtree_width / 2,
                ymin + my_size.Height / 2);

            // Increase xmin to allow room for
            // the subtree before returning.
            xmin += subtree_width;

            // Set the return value for ymin.
            ymin = biggest_ymin;

        }
    }
}

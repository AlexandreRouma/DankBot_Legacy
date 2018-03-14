using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class ResourceCache
    {
        public static Dictionary<string, ResourceItem> resources = new Dictionary<string, ResourceItem>();

        public static void Load()
        {
            resources.Add("elians_script_a", GetImageItem("resources/images/elians_script/a.png"));
            resources.Add("elians_script_b", GetImageItem("resources/images/elians_script/b.png"));
            resources.Add("elians_script_c", GetImageItem("resources/images/elians_script/c.png"));
            resources.Add("elians_script_d", GetImageItem("resources/images/elians_script/d.png"));
            resources.Add("elians_script_e", GetImageItem("resources/images/elians_script/e.png"));
            resources.Add("elians_script_f", GetImageItem("resources/images/elians_script/f.png"));
            resources.Add("elians_script_g", GetImageItem("resources/images/elians_script/g.png"));
            resources.Add("elians_script_h", GetImageItem("resources/images/elians_script/h.png"));
            resources.Add("elians_script_i", GetImageItem("resources/images/elians_script/i.png"));
            resources.Add("elians_script_j", GetImageItem("resources/images/elians_script/j.png"));
            resources.Add("elians_script_k", GetImageItem("resources/images/elians_script/k.png"));
            resources.Add("elians_script_l", GetImageItem("resources/images/elians_script/l.png"));
            resources.Add("elians_script_m", GetImageItem("resources/images/elians_script/m.png"));
            resources.Add("elians_script_n", GetImageItem("resources/images/elians_script/n.png"));
            resources.Add("elians_script_o", GetImageItem("resources/images/elians_script/o.png"));
            resources.Add("elians_script_p", GetImageItem("resources/images/elians_script/p.png"));
            resources.Add("elians_script_q", GetImageItem("resources/images/elians_script/q.png"));
            resources.Add("elians_script_r", GetImageItem("resources/images/elians_script/r.png"));
            resources.Add("elians_script_s", GetImageItem("resources/images/elians_script/s.png"));
            resources.Add("elians_script_t", GetImageItem("resources/images/elians_script/t.png"));
            resources.Add("elians_script_u", GetImageItem("resources/images/elians_script/u.png"));
            resources.Add("elians_script_v", GetImageItem("resources/images/elians_script/v.png"));
            resources.Add("elians_script_w", GetImageItem("resources/images/elians_script/w.png"));
            resources.Add("elians_script_x", GetImageItem("resources/images/elians_script/x.png"));
            resources.Add("elians_script_y", GetImageItem("resources/images/elians_script/y.png"));
            resources.Add("elians_script_z", GetImageItem("resources/images/elians_script/z.png"));
        }

        public static ResourceItem GetImageItem(string filename)
        {
            if (ConfigUtils.Configuration.ResourceCaching)
            {
                return new ResourceItem(filename, Image.FromFile(filename));
            }
            else
            {
                return new ResourceItem(filename, null);
            }
        }
    }

    class ResourceItem
    {
        public string filename;
        public object item;

        public ResourceItem(string file, object obj)
        {
            filename = file;
            item = obj;
        }
    }
}

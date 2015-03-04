namespace TShockAPI.DB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TShockAPI;

    public class Region
    {
        public Region()
        {
            this.Area = Rectangle.Empty;
            this.Name = string.Empty;
            this.DisableBuild = true;
            this.WorldID = string.Empty;
            this.AllowedIDs = new List<int>();
            this.AllowedGroups = new List<string>();
            this.Z = 0;
        }

        public Region(Rectangle region, string name, string owner, bool disablebuild, string RegionWorldIDz, int z) : this()
        {
            this.Area = region;
            this.Name = name;
            this.Owner = owner;
            this.DisableBuild = disablebuild;
            this.WorldID = RegionWorldIDz;
            this.Z = z;
        }

        public bool HasPermissionToBuildInRegion(TSPlayer ply)
        {
            if (!ply.IsLoggedIn)
            {
                if (!ply.HasBeenNaggedAboutLoggingIn)
                {
                    ply.SendMessage("You must be logged in to take advantage of protected regions.", Color.Red);
                    ply.HasBeenNaggedAboutLoggingIn = true;
                }
                return false;
            }
            return (!this.DisableBuild || ((this.AllowedIDs.Contains(ply.UserID) || this.AllowedGroups.Contains(ply.Group.Name)) || (this.Owner == ply.UserAccountName)));
        }

        public bool InArea(Rectangle point)
        {
            return this.Area.Contains(point.X, point.Y);
        }

        public bool InArea(int x, int y)
        {
            return (((x >= this.Area.Left) && (x <= this.Area.Right)) && ((y >= this.Area.Top) && (y <= this.Area.Bottom)));
        }

        public bool RemoveGroup(string groupName)
        {
            return this.AllowedGroups.Remove(groupName);
        }

        public void RemoveID(int id)
        {
            int index = -1;
            for (int i = 0; i < this.AllowedIDs.Count; i++)
            {
                if (this.AllowedIDs[i] == id)
                {
                    index = i;
                    break;
                }
            }
            this.AllowedIDs.RemoveAt(index);
        }

        public void SetAllowedGroups(string groups)
        {
            if (!string.IsNullOrEmpty(groups))
            {
                char[] separator = new char[] { ',' };
                List<string> list = groups.Split(separator).ToList<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = list[i].Trim();
                }
                this.AllowedGroups = list;
            }
        }

        public void setAllowedIDs(string ids)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = ids.Split(separator);
            List<int> list = new List<int>();
            foreach (string str in strArray)
            {
                int result = 0;
                int.TryParse(str, out result);
                if (result != 0)
                {
                    list.Add(result);
                }
            }
            this.AllowedIDs = list;
        }

        public List<string> AllowedGroups { get; set; }

        public List<int> AllowedIDs { get; set; }

        public Rectangle Area { get; set; }

        public bool DisableBuild { get; set; }

        public string Name { get; set; }

        public string Owner { get; set; }

        public string WorldID { get; set; }

        public int Z { get; set; }
    }
}

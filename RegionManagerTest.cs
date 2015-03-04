namespace TShockAPI.DB
{
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Terraria;
    using TShockAPI;

    public class RegionManager
    {
        private IDbConnection database;
        public List<Region> Regions = new List<Region>();

        internal RegionManager(IDbConnection db)
        {
            this.database = db;
            SqlColumn[] columns = new SqlColumn[12];
            SqlColumn column = new SqlColumn("Id", MySqlDbType.Int32) {
                Primary = true,
                AutoIncrement = true
            };
            columns[0] = column;
            columns[1] = new SqlColumn("X1", MySqlDbType.Int32);
            columns[2] = new SqlColumn("Y1", MySqlDbType.Int32);
            columns[3] = new SqlColumn("width", MySqlDbType.Int32);
            columns[4] = new SqlColumn("height", MySqlDbType.Int32);
            column = new SqlColumn("RegionName", MySqlDbType.VarChar, 50) {
                Unique = true
            };
            columns[5] = column;
            column = new SqlColumn("WorldID", MySqlDbType.VarChar, 50) {
                Unique = true
            };
            columns[6] = column;
            columns[7] = new SqlColumn("UserIds", MySqlDbType.Text);
            columns[8] = new SqlColumn("Protected", MySqlDbType.Int32);
            columns[9] = new SqlColumn("Groups", MySqlDbType.Text);
            columns[10] = new SqlColumn("Owner", MySqlDbType.VarChar, 50);
            column = new SqlColumn("Z", MySqlDbType.Int32) {
                DefaultValue = "0"
            };
            columns[11] = column;
            SqlTable table = new SqlTable("Regions", columns);
            new SqlTableCreator(db, (db.GetSqlType() != SqlType.Sqlite) ? ((IQueryBuilder) new MysqlQueryCreator()) : ((IQueryBuilder) new SqliteQueryCreator())).EnsureTableStructure(table);
        }

        public bool AddNewUser(string regionName, string userName)
        {
            try
            {
                string str = string.Empty;
                object[] args = new object[] { regionName, Main.worldID.ToString() };
                using (QueryResult result = this.database.QueryReader("SELECT UserIds FROM Regions WHERE RegionName=@0 AND WorldID=@1", args))
                {
                    if (result.Read())
                    {
                        str = result.Get<string>("UserIds");
                    }
                }
                string str2 = Convert.ToString(TShock.Users.GetUserID(userName));
                char[] separator = new char[] { ',' };
                if (str.Split(separator).Contains<string>(str2))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(str))
                {
                    str = str2;
                }
                else
                {
                    str = str + "," + str2;
                }
                object[] objArray2 = new object[] { str, regionName, Main.worldID.ToString() };
                int num = this.database.Query("UPDATE Regions SET UserIds=@0 WHERE RegionName=@1 AND WorldID=@2", objArray2);
                foreach (Region region in this.Regions)
                {
                    if ((region.Name == regionName) && (region.WorldID == Main.worldID.ToString()))
                    {
                        region.setAllowedIDs(str);
                    }
                }
                return (num != 0);
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
            }
            return false;
        }

        public bool AddRegion(int tx, int ty, int width, int height, string regionname, string owner, string worldid, int z = 0)
        {
            if (this.GetRegionByName(regionname) == null)
            {
                try
                {
                    object[] args = new object[] { tx, ty, width, height, regionname, worldid, string.Empty, 1, string.Empty, owner, z };
                    this.database.Query("INSERT INTO Regions (X1, Y1, width, height, RegionName, WorldID, UserIds, Protected, Groups, Owner, Z) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10);", args);
                    this.Regions.Add(new Region(new Rectangle(tx, ty, width, height), regionname, owner, true, worldid, z));
                    return true;
                }
                catch (Exception exception)
                {
                    Log.Error(exception.ToString());
                }
            }
            return false;
        }

        public bool AllowGroup(string regionName, string groupName)
        {
            string groups = string.Empty;
            object[] args = new object[] { regionName, Main.worldID.ToString() };
            using (QueryResult result = this.database.QueryReader("SELECT Groups FROM Regions WHERE RegionName=@0 AND WorldID=@1", args))
            {
                if (result.Read())
                {
                    groups = result.Get<string>("Groups");
                }
            }
            char[] separator = new char[] { ',' };
            if (groups.Split(separator).Contains<string>(groupName))
            {
                return true;
            }
            if (groups != string.Empty)
            {
                groups = groups + ",";
            }
            groups = groups + groupName;
            object[] objArray2 = new object[] { groups, regionName, Main.worldID.ToString() };
            int num = this.database.Query("UPDATE Regions SET Groups=@0 WHERE RegionName=@1 AND WorldID=@2", objArray2);
            Region regionByName = this.GetRegionByName(regionName);
            if (regionByName != null)
            {
                regionByName.SetAllowedGroups(groups);
                return (num > 0);
            }
            return false;
        }

        public bool CanBuild(int x, int y, TSPlayer ply)
        {
            if (!ply.Group.HasPermission(Permissions.canbuild))
            {
                return false;
            }
            Region region = null;
            foreach (Region region2 in this.Regions.ToList<Region>())
            {
                if (region2.InArea(x, y) && ((region == null) || (region2.Z > region.Z)))
                {
                    region = region2;
                }
            }
            return ((region == null) || region.HasPermissionToBuildInRegion(ply));
        }

        public bool ChangeOwner(string regionName, string newOwner)
        {
            Region regionByName = this.GetRegionByName(regionName);
            if (regionByName != null)
            {
                regionByName.Owner = newOwner;
                object[] args = new object[] { newOwner, regionName, Main.worldID.ToString() };
                if (this.database.Query("UPDATE Regions SET Owner=@0 WHERE RegionName=@1 AND WorldID=@2", args) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool DeleteRegion(string name)
        {
            <DeleteRegion>c__AnonStorey2 storey = new <DeleteRegion>c__AnonStorey2 {
                name = name
            };
            try
            {
                <DeleteRegion>c__AnonStorey3 storey2 = new <DeleteRegion>c__AnonStorey3 {
                    <>f__ref$2 = storey
                };
                object[] args = new object[] { storey.name, Main.worldID.ToString() };
                this.database.Query("DELETE FROM Regions WHERE RegionName=@0 AND WorldID=@1", args);
                storey2.worldid = Main.worldID.ToString();
                this.Regions.RemoveAll(new Predicate<Region>(storey2.<>m__0));
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
            }
            return false;
        }

        public Region GetRegionByName(string name)
        {
            <GetRegionByName>c__AnonStorey6 storey = new <GetRegionByName>c__AnonStorey6 {
                name = name
            };
            return this.Regions.FirstOrDefault<Region>(new Func<Region, bool>(storey.<>m__3));
        }

        public Region GetTopRegion(List<Region> regions)
        {
            Region region = null;
            foreach (Region region2 in regions)
            {
                if (region == null)
                {
                    region = region2;
                }
                else if (region2.Z > region.Z)
                {
                    region = region2;
                }
            }
            return region;
        }

        public bool InArea(int x, int y)
        {
            foreach (Region region in this.Regions.ToList<Region>())
            {
                if ((((x >= region.Area.Left) && (x <= region.Area.Right)) && ((y >= region.Area.Top) && (y <= region.Area.Bottom))) && region.DisableBuild)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Region> InAreaRegion(int x, int y)
        {
            List<Region> list = new List<Region>();
            foreach (Region region in this.Regions.ToList<Region>())
            {
                if ((((x >= region.Area.Left) && (x <= region.Area.Right)) && ((y >= region.Area.Top) && (y <= region.Area.Bottom))) && region.DisableBuild)
                {
                    list.Add(region);
                }
            }
            return list;
        }

        public List<string> InAreaRegionName(int x, int y)
        {
            List<string> list = new List<string>();
            foreach (Region region in this.Regions.ToList<Region>())
            {
                if ((((x >= region.Area.Left) && (x <= region.Area.Right)) && ((y >= region.Area.Top) && (y <= region.Area.Bottom))) && region.DisableBuild)
                {
                    list.Add(region.Name);
                }
            }
            return list;
        }

        public List<Region> ListAllRegions(string worldid)
        {
            List<Region> list = new List<Region>();
            try
            {
                object[] args = new object[] { worldid };
                using (QueryResult result = this.database.QueryReader("SELECT RegionName FROM Regions WHERE WorldID=@0", args))
                {
                    while (result.Read())
                    {
                        Region item = new Region {
                            Name = result.Get<string>("RegionName")
                        };
                        list.Add(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
            }
            return list;
        }

        public static List<string> ListIDs(string MergedIDs)
        {
            char[] separator = new char[] { ',' };
            return MergedIDs.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
        }

        public bool PositionRegion(string regionName, int x, int y, int width, int height)
        {
            <PositionRegion>c__AnonStorey5 storey = new <PositionRegion>c__AnonStorey5 {
                regionName = regionName
            };
            try
            {
                this.Regions.First<Region>(new Func<Region, bool>(storey.<>m__2)).Area = new Rectangle(x, y, width, height);
                object[] args = new object[] { x, y, width, height, storey.regionName, Main.worldID.ToString() };
                if (this.database.Query("UPDATE Regions SET X1 = @0, Y1 = @1, width = @2, height = @3 WHERE RegionName = @4 AND WorldID = @5", args) > 0)
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
            }
            return false;
        }

        public void Reload()
        {
            try
            {
                object[] args = new object[] { Main.worldID.ToString() };
                using (QueryResult result = this.database.QueryReader("SELECT * FROM Regions WHERE WorldID=@0", args))
                {
                    this.Regions.Clear();
                    while (result.Read())
                    {
                        int x = result.Get<int>("X1");
                        int y = result.Get<int>("Y1");
                        int height = result.Get<int>("height");
                        int width = result.Get<int>("width");
                        int num5 = result.Get<int>("Protected");
                        string str = result.Get<string>("UserIds");
                        string name = result.Get<string>("RegionName");
                        string owner = result.Get<string>("Owner");
                        string groups = result.Get<string>("Groups");
                        int z = result.Get<int>("Z");
                        char[] separator = new char[] { ',' };
                        string[] strArray = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        Region item = new Region(new Rectangle(x, y, width, height), name, owner, num5 != 0, Main.worldID.ToString(), z);
                        item.SetAllowedGroups(groups);
                        try
                        {
                            for (int i = 0; i < strArray.Length; i++)
                            {
                                int num8;
                                if (int.TryParse(strArray[i], out num8))
                                {
                                    item.AllowedIDs.Add(num8);
                                }
                                else
                                {
                                    Log.Warn("One of your UserIDs is not a usable integer: " + strArray[i]);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.Error("Your database contains invalid UserIDs (they should be ints).");
                            Log.Error("A lot of things will fail because of this. You must manually delete and re-create the allowed field.");
                            Log.Error(exception.ToString());
                            Log.Error(exception.StackTrace);
                        }
                        this.Regions.Add(item);
                    }
                }
            }
            catch (Exception exception2)
            {
                Log.Error(exception2.ToString());
            }
        }

        public bool RemoveGroup(string regionName, string group)
        {
            Region regionByName = this.GetRegionByName(regionName);
            if (regionByName != null)
            {
                regionByName.RemoveGroup(group);
                string str = string.Join(",", regionByName.AllowedGroups);
                object[] args = new object[] { str, regionName, Main.worldID.ToString() };
                if (this.database.Query("UPDATE Regions SET Groups=@0 WHERE RegionName=@1 AND WorldID=@2", args) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool RemoveUser(string regionName, string userName)
        {
            Region regionByName = this.GetRegionByName(regionName);
            if (regionByName != null)
            {
                regionByName.RemoveID(TShock.Users.GetUserID(userName));
                string str = string.Join<int>(",", regionByName.AllowedIDs);
                object[] args = new object[] { str, regionName, Main.worldID.ToString() };
                if (this.database.Query("UPDATE Regions SET UserIds=@0 WHERE RegionName=@1 AND WorldID=@2", args) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool resizeRegion(string regionName, int addAmount, int direction)
        {
            <resizeRegion>c__AnonStorey4 storey = new <resizeRegion>c__AnonStorey4 {
                regionName = regionName
            };
            int x = 0;
            int y = 0;
            int height = 0;
            int width = 0;
            try
            {
                object[] args = new object[] { storey.regionName, Main.worldID.ToString() };
                using (QueryResult result = this.database.QueryReader("SELECT X1, Y1, height, width FROM Regions WHERE RegionName=@0 AND WorldID=@1", args))
                {
                    if (result.Read())
                    {
                        x = result.Get<int>("X1");
                        width = result.Get<int>("width");
                        y = result.Get<int>("Y1");
                        height = result.Get<int>("height");
                    }
                }
                switch (direction)
                {
                    case 0:
                        y -= addAmount;
                        height += addAmount;
                        break;

                    case 1:
                        width += addAmount;
                        break;

                    case 2:
                        height += addAmount;
                        break;

                    case 3:
                        x -= addAmount;
                        width += addAmount;
                        break;

                    default:
                        return false;
                }
                foreach (Region region in this.Regions.Where<Region>(new Func<Region, bool>(storey.<>m__1)))
                {
                    region.Area = new Rectangle(x, y, width, height);
                }
                object[] objArray2 = new object[] { x, y, width, height, storey.regionName, Main.worldID.ToString() };
                if (this.database.Query("UPDATE Regions SET X1 = @0, Y1 = @1, width = @2, height = @3 WHERE RegionName = @4 AND WorldID=@5", objArray2) > 0)
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
            }
            return false;
        }

        public bool SetRegionState(string name, bool state)
        {
            try
            {
                object[] args = new object[] { !state ? 0 : 1, name, Main.worldID.ToString() };
                this.database.Query("UPDATE Regions SET Protected=@0 WHERE RegionName=@1 AND WorldID=@2", args);
                Region regionByName = this.GetRegionByName(name);
                if (regionByName != null)
                {
                    regionByName.DisableBuild = state;
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
                return false;
            }
        }

        public bool SetRegionStateTest(string name, string world, bool state)
        {
            try
            {
                object[] args = new object[] { !state ? 0 : 1, name, world };
                this.database.Query("UPDATE Regions SET Protected=@0 WHERE RegionName=@1 AND WorldID=@2", args);
                Region regionByName = this.GetRegionByName(name);
                if (regionByName != null)
                {
                    regionByName.DisableBuild = state;
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
                return false;
            }
        }

        public bool SetZ(string name, int z)
        {
            try
            {
                object[] args = new object[] { z, name, Main.worldID.ToString() };
                this.database.Query("UPDATE Regions SET Z=@0 WHERE RegionName=@1 AND WorldID=@2", args);
                Region regionByName = this.GetRegionByName(name);
                if (regionByName != null)
                {
                    regionByName.Z = z;
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception.ToString());
                return false;
            }
        }

        [CompilerGenerated]
        private sealed class <DeleteRegion>c__AnonStorey2
        {
            internal string name;
        }

        [CompilerGenerated]
        private sealed class <DeleteRegion>c__AnonStorey3
        {
            internal RegionManager.<DeleteRegion>c__AnonStorey2 <>f__ref$2;
            internal string worldid;

            internal bool <>m__0(Region r)
            {
                return ((r.Name == this.<>f__ref$2.name) && (r.WorldID == this.worldid));
            }
        }

        [CompilerGenerated]
        private sealed class <GetRegionByName>c__AnonStorey6
        {
            internal string name;

            internal bool <>m__3(Region r)
            {
                return (r.Name.Equals(this.name) && (r.WorldID == Main.worldID.ToString()));
            }
        }

        [CompilerGenerated]
        private sealed class <PositionRegion>c__AnonStorey5
        {
            internal string regionName;

            internal bool <>m__2(Region r)
            {
                return string.Equals(this.regionName, r.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        [CompilerGenerated]
        private sealed class <resizeRegion>c__AnonStorey4
        {
            internal string regionName;

            internal bool <>m__1(Region r)
            {
                return (r.Name == this.regionName);
            }
        }
    }
}


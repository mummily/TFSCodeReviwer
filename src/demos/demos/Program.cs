using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace TFSCodeReviewer
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {

        private static string s_uri = @"http://tfs02.hollysys.net:9080/tfs/";



        static void Main(string[] args)
        {
            //测试获取项目名称
            TeamManager mgr = new TeamManager();
            List<TeamInfo> projs = mgr.GetTeams(s_uri);
            TEST_PrintProjectInfos(projs);

            TeamInfo team = projs.Find(s => s.TeamCollectionName.Contains("v6.5.x"));

            ItemSet items = TEST_GetItems(team, "$/HMI/01_dev-V6.5.1/服务器/代码/NewAlarmSummery");

            PrintItems(items);

            string itemPath = "$/HMI/01_dev-V6.5.1/服务器/代码/NewAlarmSummery/NewAlarmSummery.cpp";

            IEnumerable chgs = TEST_GetHistorys(team, itemPath);

            Item item = TEST_GetItem(team, itemPath);

            // 最新的变更集.
            Changeset latest = null;

            // 最新的变更集后面那个变更集.
            Changeset later = null;

            int pos = 0;

            foreach (Changeset chg in chgs)
            {
                Changeset chgset = (Changeset)chg;
                Console.Write(chgset.ChangesetId);
                Console.Write(" : ");
                Console.WriteLine(chgset.Comment);

                if (pos == 0)
                {
                    latest = chg;
                }
                else if (pos == 4)
                {
                    later = chg;
                }

                ++pos;
            }

            DiffItemVersionedFile srcFile = new DiffItemVersionedFile(item, new ChangesetVersionSpec(latest.ChangesetId));
            DiffItemVersionedFile dstFile = new DiffItemVersionedFile(item, new ChangesetVersionSpec(later.ChangesetId));

            DiffOptions diffOpts = new DiffOptions();
            diffOpts.Flags = diffOpts.Flags | DiffOptionFlags.None;

            Difference.VisualDiffFiles(item.VersionControlServer, itemPath, new ChangesetVersionSpec(latest.ChangesetId),
                itemPath, new ChangesetVersionSpec(later.ChangesetId));
            //Difference.DiffFiles(item.VersionControlServer, srcFile, dstFile, diffOpts, null, true);

            item.VersionControlServer.DownloadFile(item.ServerItem.ToString(), 0, new ChangesetVersionSpec(latest.ChangesetId), "./temp1.cpp");
            item.VersionControlServer.DownloadFile(item.ServerItem.ToString(), 0, new ChangesetVersionSpec(later.ChangesetId), "./temp2.cpp");

            DiffSegment diffs = Difference.DiffFiles("./temp1.cpp", item.Encoding, "./temp2.cpp", item.Encoding, diffOpts);

            Console.ReadLine();

        }


        /// <summary>
        /// 获取项目信息
        /// </summary>
        /// <param name="uri">TFS地址</param>
        private static void TEST_PrintProjectInfos(List<TeamInfo> projects)
        {
            try
            {
                foreach (var item in projects)
                {
                    Console.WriteLine(Utility.SplitOutLastSubstr(Uri.UnescapeDataString(item.TeamCollectionName), new char[2] {'\\', '/'}));

                    foreach (var project in item.ProjectNames)
                    {
                        Console.WriteLine("\t" + Utility.SplitOutLastSubstr(Uri.UnescapeDataString(project.ProjectName), new char[2] {'\\', '/'}));
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static Item TEST_GetItem(TeamInfo team, string path)
        {
            Item item = null;
            try
            {
                VersionControlServer vcs = null;
                team.VerCtrls.TryGetValue(team.TeamCollectionName, out vcs);
                item = vcs.GetItem(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return item;
        }

        private static ItemSet TEST_GetItems(TeamInfo team, string path)
        {
            ItemSet sets = null;
            try
            {
                VersionControlServer vcs = null;
                team.VerCtrls.TryGetValue(team.TeamCollectionName, out vcs);
                sets = vcs.GetItems(path, RecursionType.Full);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return sets;
        }

        private static void PrintItems(ItemSet items)
        {
            foreach (var item in items.Items)
            {
                Console.Write(item.ItemType.ToString());
                Console.Write(": ");
                Console.WriteLine(item.ServerItem.ToString());
            }
        }

        private static IEnumerable TEST_GetHistorys(TeamInfo team, string path)
        {
            VersionControlServer vcs = null;
            team.VerCtrls.TryGetValue(team.TeamCollectionName, out vcs);
            Item item = TEST_GetItem(team, path);
            ChangesetVersionSpec verspec = new ChangesetVersionSpec(item.ChangesetId);
            var chgs = vcs.QueryHistory(path, verspec, 0, RecursionType.Full, null, null, null, 5, true, false);
            return chgs;
        }
    }
}

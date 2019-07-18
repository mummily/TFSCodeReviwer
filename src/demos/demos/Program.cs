using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Forms;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TFSCodeCounter
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CounterConfig config = GetConfig();

            DownloadFiles(config, "52100");
            DiffCount(config);

            Console.WriteLine("Press Any Key to Exit ... ...");
            Console.ReadKey();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="changsetID"></param>
        private static void DownloadFiles(CounterConfig config, string changsetID)
        {
            try
            {
                TfsConfigurationServer configurationServer =
                  TfsConfigurationServerFactory.GetConfigurationServer(new Uri(config.Tfs));

                // Get the catalog of team project collections
                ReadOnlyCollection<CatalogNode> collectionNodes = configurationServer.CatalogNode.QueryChildren(
                  new[] { CatalogResourceTypes.ProjectCollection },
                  false, CatalogQueryOptions.None);

                // List the team project collections
                VersionControlServer vcs = null;
                foreach (CatalogNode collectionNode in collectionNodes)
                {
                    // Use the InstanceId property to get the team project collection
                    Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
                    TfsTeamProjectCollection teamProjectCollection = configurationServer.GetTeamProjectCollection(collectionId);

                    if (!teamProjectCollection.Name.Equals(config.Project))
                        continue;

                    vcs = teamProjectCollection.GetService<VersionControlServer>();
                    break;
                }

                if (vcs == null)
                    throw new Exception();

                var changesetList = vcs.QueryHistory(
                  config.ServerLocation,
                  VersionSpec.Latest,
                  0,
                  RecursionType.Full,
                  null,
                  null,
                  VersionSpec.ParseSingleSpec(changsetID, null),
                  1,
                  true,
                  false).Cast<Changeset>();

                foreach (var cs in changesetList)
                {
                    var change = cs.Changes;
                    foreach (var itemList in change)
                    {
                        if (itemList.Item == null)
                            continue;

                        string ServerItem = itemList.Item.ServerItem;

                        var itemChgs = vcs.QueryHistory(itemList.Item.ServerItem,
                            VersionSpec.Latest,
                            0,
                            RecursionType.Full,
                            null,
                            null,
                            VersionSpec.ParseSingleSpec(changsetID, null),
                            2,
                            true,
                            false);

                        int index = 0;
                        foreach (Changeset itemchg in itemChgs)
                        {
                            string revisionpath = (index == 0) ? config.CurrentRevision : config.PreviousRevision;
                            string localItem = config.ClientLocation + @"\" + revisionpath + @"\" + changsetID;
                            localItem += ServerItem.Substring(config.ServerLocation.Length);
                            //public enum ChangeType
                            //{
                            //    None = 1,
                            //    Add = 2,
                            //    Edit = 4,
                            //    Encoding = 8,
                            //    Rename = 16,
                            //    Delete = 32,
                            //    Undelete = 64,
                            //    Branch = 128,
                            //    Merge = 256,
                            //    Lock = 512,
                            //    Rollback = 1024,
                            //    SourceRename = 2048,
                            //    Property = 8192
                            //}

                            if (!itemchg.Changes[0].ChangeType.HasFlag(ChangeType.Delete))
                                vcs.DownloadFile(ServerItem, 0, VersionSpec.ParseSingleSpec(Convert.ToString(itemchg.ChangesetId), null), localItem);

                            ++index;
                        }
                    }
                }
            }
            catch (ChangesetNotFoundException)
            {
                Console.WriteLine("!! Please check the change set id inside your config file !!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static CounterConfig GetConfig()
        {
            string file = Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);

            CounterConfig counterCfg = new CounterConfig();
            counterCfg.Tfs = config.AppSettings.Settings["Tfs"].Value.ToString();
            counterCfg.Project = config.AppSettings.Settings["Project"].Value.ToString();
            counterCfg.ServerLocation = config.AppSettings.Settings["ServerLocation"].Value.ToString();
            counterCfg.ClientLocation = config.AppSettings.Settings["ClientLocation"].Value.ToString();
            counterCfg.CurrentRevision = config.AppSettings.Settings["CurrentRevision"].Value.ToString();
            counterCfg.PreviousRevision = config.AppSettings.Settings["PreviousRevision"].Value.ToString();
            counterCfg.OutputFile = config.AppSettings.Settings["OutputFile"].Value.ToString();
            counterCfg.IsRemain = Convert.ToBoolean(config.AppSettings.Settings["IsRemain"].Value);

            return counterCfg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        private static void DiffCount(CounterConfig config)
        {
            string cmd = "diffcount.exe ";
            cmd += config.ClientLocation + @"\" + config.PreviousRevision + " " + config.ClientLocation + @"\" + config.CurrentRevision;
            cmd += " > " + config.OutputFile;

            Process proc = new Process();            proc.StartInfo.CreateNoWindow = true;            proc.StartInfo.FileName = "cmd.exe";            proc.StartInfo.UseShellExecute = false;            proc.StartInfo.RedirectStandardError = true;            proc.StartInfo.RedirectStandardInput = true;            proc.StartInfo.RedirectStandardOutput = true;            proc.Start();            proc.StandardInput.WriteLine(cmd);            proc.StandardInput.WriteLine("exit");            proc.Close();
        }
    }
}

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace TFSCodeReviewer
{

    public class ProjectIno
    {
        public string ProjectName { get; set; }
    }

    public class TeamInfo
    {
        public string TeamCollectionName { get; set; }
        public List<ProjectIno> ProjectNames { get; set; }
        public Dictionary<string, VersionControlServer> VerCtrls { get; set; }
    }

    public class TeamManager
    {
        public List<TeamInfo> GetTeams(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException(uri);
            }

            List<TeamInfo> teams = new List<TeamInfo>(16);

            Uri tfsUri = new Uri(uri);
            TfsConfigurationServer configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(tfsUri);

            // Get the catalog of team project collections
            ReadOnlyCollection<CatalogNode> collectionNodes = configurationServer.CatalogNode.QueryChildren(
                new[] { CatalogResourceTypes.ProjectCollection },
                false, CatalogQueryOptions.None);

            // List the team project collections
            foreach (CatalogNode collectionNode in collectionNodes)
            {
                // Use the InstanceId property to get the team project collection
                Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
                TfsTeamProjectCollection teamProjectCollection = configurationServer.GetTeamProjectCollection(collectionId);
                VersionControlServer vcs = teamProjectCollection.GetService<VersionControlServer>();

                TeamInfo ti = new TeamInfo();
                ti.VerCtrls = new Dictionary<string, VersionControlServer>();
                ti.TeamCollectionName = teamProjectCollection.Name;
                ti.VerCtrls.Add(ti.TeamCollectionName, vcs);

                // Get a catalog of team projects for the collection
                ReadOnlyCollection<CatalogNode> projectNodes = collectionNode.QueryChildren(
                    new[] { CatalogResourceTypes.TeamProject },
                    false, CatalogQueryOptions.None);

                ti.ProjectNames = new List<ProjectIno>();

                // List the team projects in the collection
                foreach (CatalogNode projectNode in projectNodes)
                {
                    ti.ProjectNames.Add(new ProjectIno { ProjectName = projectNode.Resource.DisplayName });
                }

                teams.Add(ti);
            }

            return teams;
        }
    }
}

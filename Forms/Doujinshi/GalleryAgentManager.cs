using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader.Doujinshi
{
    public class GalleryAgentManager
    {
        private List<GalleryAgent> Agents { get; }

        public HitomiAgent Hitomi { get; }
        public ExHentaiAgent ExHentai { get; }

        public GalleryAgentManager()
        {
            this.Agents = new List<GalleryAgent>();
            this.Hitomi = this.Register(new HitomiAgent());

            var account = DoujinshiDownloader.Instance.Config.Values.Agent.ExHentaiAccount;
            this.ExHentai = this.Register(new ExHentaiAgent() { Account = account });
        }

        public GalleryAgent[] GetAgents()
        {
            var agents = this.Agents;

            lock (agents)
            {
                return agents.ToArray();
            }

        }

        public T Register<T>(T agent) where T : GalleryAgent
        {
            var agents = this.Agents;

            lock (agents)
            {
                if (agents.Contains(agent) == false)
                {
                    agents.Add(agent);
                }

            }

            return agent;
        }

    }

}

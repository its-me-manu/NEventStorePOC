using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace NEventStorePOC.Framework
{
    public class CommandExtensions
    {
        [ImportMany(typeof(ICommandMaterializationExtension))]
        public IEnumerable<ICommandMaterializationExtension> Extensions { get; set; }

        public CommandExtensions()
        {
            using (var catalog = new AssemblyCatalog(typeof(Command).Assembly))
            {
                using (var container = new CompositionContainer(catalog))
                {
                    container.ComposeParts(this);
                }
            }
        }
    }
}
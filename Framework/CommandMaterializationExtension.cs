using System.ComponentModel.Composition;

namespace NEventStorePOC.Framework
{
     [InheritedExport(typeof(ICommandMaterializationExtension))]
    public interface ICommandMaterializationExtension
    {
        Type MaterializedObjectType { get; }
    }

    public abstract class CommandMaterializationExtension<T> : ICommandMaterializationExtension
    {
        public Type MaterializedObjectType => typeof(T);

        public abstract void AfterMaterialization(Command<T> command, T materializedObject);
    }
}
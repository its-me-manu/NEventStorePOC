using NEventStorePOC.Framework;

namespace NEventStorePOC.Model
{
    public class CommandJSON
    {
        public Guid id { get; set; }
        public dynamic JSONCommand { get; set; }
        public Command Command { get; set; }
    }
}
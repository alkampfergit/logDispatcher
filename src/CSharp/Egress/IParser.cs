namespace Egress
{
    /// <summary>
    /// Simple parser that will take the original line from logfile (or generic log source)
    /// and simply convert in a nice json formatted string to be stored in Mongodb
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parse a line to a json serialized as string.
        /// </summary>
        /// <param name="source">Name of the source, used to separate log in the receiver.</param>
        /// <param name="line"></param>
        /// <returns></returns>
        string Parse(string source, string line);
    }
}

namespace file_distributor
{
    /// <summary>
    /// Contains information about arguments provided by the user
    /// </summary>
    internal struct Argument
    {
        /// <summary>
        /// The name/key for the argument
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The value specified for the argument.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Create an simple switch argument
        /// </summary>
        /// <param name="name">Name/key of the argument</param>
        public Argument(string name)
        {
            Name = name; 
            Value = "true";
        }

        /// <summary>
        /// Create a standard argument with value
        /// </summary>
        /// <param name="name">Name/key of the argument</param>
        /// <param name="value">Value of the argument</param>
        public Argument(string name, string value)
        {
            Name = name; Value = value;
        }
    }
}

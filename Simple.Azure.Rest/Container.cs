﻿namespace Simple.Azure.Rest
{
    using System;
    using System.Collections.Generic;

    public class Container
    {
        public Container()
        {
            Metadata = new Dictionary<string, string>();
        }
        public string Name { get; set; }
        public string Url { get; set; }
        public IDictionary<string,string> Metadata { get; set; }
        public ContainerProperties Properties { get; set; }
        public string Access { get; set; }
    }
}
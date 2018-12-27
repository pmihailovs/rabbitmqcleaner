using System;
using System.Text;

namespace ExchangeCleaner.Data
{
    public class RabbitExchangeDefinition : IRabbitExchangeDefinition
    {
        private string _fullDotNetName;
        private string _shortDotNetName;
        private string _genericDefinedFullDotNetName;

        public RabbitExchangeDefinition()
        {
        }

        public RabbitExchangeDefinition(string rabbitName)
        {
            this.RabbitExchangeName = rabbitName;
        }

        public string RabbitExchangeName { get; set; }

        public string FullDotNetName
        {
            get { return this._fullDotNetName; }
            set
            {
                this._fullDotNetName = value;
                this.ShortDotNetName = TrimNamespaces(value);
                this._genericDefinedFullDotNetName = TrimTypeDefinition(value);
            }
        }

        private string TrimTypeDefinition(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var lastGenericOpening = value.LastIndexOf('<');

            if (lastGenericOpening == -1)
            {
                return value;
            }

            var firstGenericClosing = value.IndexOf('>');

            var classDefinition = value.Substring(lastGenericOpening + 1, firstGenericClosing - lastGenericOpening - 1);
            return classDefinition;
        }

        public string ShortDotNetName
        {
            get { return this._shortDotNetName; }
            private set { this._shortDotNetName = value; }
        }

        public string GenericDefinedFullDotNetName
        {
            get { return this._genericDefinedFullDotNetName; }
            private set { this._genericDefinedFullDotNetName = value; }
        }

        public string VirtualHostName { get; set; }

        private string TrimNamespaces(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.IndexOf('.') == -1)
            {
                return value;
            }

            var buffer = new StringBuilder();
            char letter;

            var namespaceOpen = false;

            for (int i = value.Length - 1; i > -1; i--)
            {
                letter = value[i];

                switch (letter)
                {
                    case '.':
                        namespaceOpen = true;
                        break;
                    case '<':
                    case '>':
                        {
                            buffer.Append(letter);
                            namespaceOpen = false;
                        }
                        break;
                    default:
                        {
                            if (!namespaceOpen)
                            {
                                buffer.Append(letter);
                            }
                        }
                        break;
                }
            }

            return buffer.Reverse().ToString();
        }

        public override bool Equals(object obj)
        {
            var exchange = obj as RabbitExchangeDefinition;
            if (exchange != null)
            {
                return exchange.FullDotNetName == this.FullDotNetName && exchange.ShortDotNetName == this.ShortDotNetName && this.RabbitExchangeName == exchange.RabbitExchangeName && this.VirtualHostName == exchange.VirtualHostName;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return RabbitExchangeName.GetHashCode() ^ FullDotNetName.GetHashCode() ^ ShortDotNetName.GetHashCode() ^ VirtualHostName.GetHashCode();
        }
    }
}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections.Generic;
using System.IO;

namespace EMP.Cs
{
    public abstract class TemplateParser {

        protected readonly string template;
        private StringReader reader;
        private string peek;
        private bool eof;
        private int lineNr;

        public TemplateParser(string templateCode)
        {
            this.template = templateCode;
        }

        protected string PeekLine()
        {
            if (peek != null)
            {
                return peek;
            } else
            {
                peek = ReadLineInternal();
                return peek;
            }

        }
        protected string ReadLine()
        {
            if (peek != null)
            {
                string tmp = peek;
                peek = null;
                return tmp;
            }
            return ReadLineInternal();
        }

        protected int GetLineNr()
        {
            return lineNr;
        }

        private string ReadLineInternal()
        {
            if (reader == null)
            {
                reader = new StringReader(template);
            }
            if (!eof)
            {
                lineNr++;
                string line = reader.ReadLine();
                if (line == null)
                {
                    reader.Close();
                    eof = true;
                }
                return line;
            }
            else
            {
                return null;
            }
        }

        public abstract Template Parse();
    }

    public class ParseNode
    {
        internal int lineNr = -1;
    }

    public class TextNode : ParseNode
    {
        internal string text;
    }

    public class Block : ParseNode
    {
        internal string leadingSpaces;
        internal string name;
        internal List<ParseNode> childNodes = new List<ParseNode>();
    }

    public class Template : Block
    {
        internal Dictionary<string, Block> blocks = new Dictionary<string, Block>();
    }

    public class ForeachWithBlock : ParseNode
    {
        internal BlockRef blockRef;
        internal string dataKey;
    }

    public class BlockRef : ParseNode
    {
        internal string blockName;
        internal Block Find(Template template)
        {
            if (!template.blocks.ContainsKey(blockName))
            {
                throw new CodeGenerationException(this, "Unknown block reference: '" + blockName + "'.");
            }
            return template.blocks[blockName];
        }
    }

    public class InsertBlock : ParseNode
    {
        internal BlockRef blockRef;
        internal string leadingSpaces;
    }

    public class If : ParseNode
    {
        internal List<Case> cases = new List<Case>();
    }

    public class Case : ParseNode
    {
        internal Condition condition;
        internal Block block;
    }

    public class Condition : ParseNode
    {
        internal string varname;
        internal string value;
        internal bool unequal;
        internal virtual bool IsTrue(Dictionary<string, object> data)
        {
            if (!data.ContainsKey(varname))
            {
                throw new CodeGenerationException(this, "Variable with name '" + varname + "' ist not set in the current scope.");
            }
            bool result = data[varname].ToString().Equals(value);
            return unequal ? !result : result;
        }
    }

    public class CodeGenerationException : Exception
    {
        public CodeGenerationException(ParseNode node, string message) : base("Line " + node.lineNr + ": " + message) { }
    }

    public class ParserException : Exception
    {
        public ParserException(ParseNode node, string message) : base("Line " + node.lineNr + ": " + message) { }
    }
}

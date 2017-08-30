//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Text;
using System.Text.RegularExpressions;

namespace EMP.Cs
{
    public class DefaultTemplateParser : TemplateParser
    {
        private static readonly Regex PATTERN_BEGIN_BLOCK =
            new Regex(@"^(\s*)(?://)?\s*#block\s+([a-zA-Z_]+)\s*$");
        private const int
            GROUP_BEGIN_BLOCK_LEADING = 1,
            GROUP_BEGIN_BLOCK_NAME = 2;
         
        private static readonly Regex PATTERN_END_BLOCK =
                    new Regex(@"^\s*(?://)?\s*#endblock\s*$");

        private static readonly Regex PATTERN_FOREACH_WITH_BLOCK =
            new Regex(@"^(\s*)(?://)?\s*#foreach\s+in\s+([a-zA-Z_]+)\s+with\s+([a-zA-Z_]+)\s*$");
        private const int
            GROUP_FOREACH_WITH_BLOCK_LEADING = 1,
            GROUP_FOREACH_WITH_BLOCK_KEY = 2,
            GROUP_FOREACH_WITH_BLOCK_BLOCKREF = 3;

        private static readonly Regex PATTERN_INSERT_BLOCK =
            new Regex(@"^(\s*)(?://)?\s*#insertblock\s+([a-zA-Z_]+)\s*$");
                private const int
                    GROUP_INSERT_BLOCK_LEADING = 1,
                    GROUP_INSERT_BLOCK_BLOCKREF = 2;

        private static readonly Regex PATTERN_IF =
            new Regex(@"^(\s*)(?://)?\s*#if\s+([a-zA-Z_]+)\s*(==|!=)\s*([1-2a-zA-Z_]+)\s*$");
                private const int
                    GROUP_IF_LEADING = 1,
                    GROUP_IF_VARNAME = 2,
                    GROUP_IF_OPERATOR = 3,
                    GROUP_IF_VALUE = 4;

        private static readonly Regex PATTERN_ELSEIF =
            new Regex(@"^(\s*)(?://)?\s*#elseif\s+([a-zA-Z_]+)\s*(==|!=)\s*([1-2a-zA-Z_]+)\s*$");

        private static readonly Regex PATTERN_ELSE =
            new Regex(@"^\s*(?://)?\s*#else\s*$");

        private static readonly Regex PATTERN_ENDIF =
            new Regex(@"^\s*(?://)?\s*#endif\s*$");

        private readonly Regex[] symbols = { PATTERN_BEGIN_BLOCK, PATTERN_END_BLOCK,
                                             PATTERN_FOREACH_WITH_BLOCK, PATTERN_INSERT_BLOCK,
                                             PATTERN_IF, PATTERN_ELSEIF, PATTERN_ELSE, PATTERN_ENDIF };

        public DefaultTemplateParser(string templateCode) : base(templateCode) { }

        public override Template Parse()
        {
            Template result = new Template();
            result.name = "";
            ReadBlockContent(result, result);
            return result;
        }

        private void ReadBlockContent(Template template, Block block)
        {
            string line;
            while((line = PeekLine()) != null)
            {
                if (PATTERN_BEGIN_BLOCK.IsMatch(line))
                {
                    block.childNodes.Add(ReadBlock(template));
                }
                else if(PATTERN_END_BLOCK.IsMatch(line))
                {
                    ReadLine();
                    return;
                }
                else if (PATTERN_ELSEIF.IsMatch(line) || PATTERN_ELSE.IsMatch(line) || PATTERN_ENDIF.IsMatch(line))
                {
                    return;
                }
                else if (PATTERN_FOREACH_WITH_BLOCK.IsMatch(line))
                {
                    block.childNodes.Add(ReadForeachWithBlock());
                }
                else if (PATTERN_INSERT_BLOCK.IsMatch(line))
                {
                    block.childNodes.Add(ReadInsertBlock());
                }
                else if (PATTERN_IF.IsMatch(line))
                {
                    block.childNodes.Add(ReadIfStatement(template));
                }
                else
                {
                    block.childNodes.Add(ReadText());
                }
            }
        }

        private If ReadIfStatement(Template template)
        {
            If result = new If();
            result.lineNr = GetLineNr();

            bool elseCaseAlreadyFound = false;
            while(true)
            {
                string line = ReadLine();
                bool isElseIfMatch = false;
                if (line == null)
                {
                    throw new ParserException(result, "Unexpected EOF while reading if statements. Missing #endif?");
                }
                if (PATTERN_ENDIF.IsMatch(line))
                {
                    break;
                } else if (PATTERN_IF.IsMatch(line) || (isElseIfMatch = PATTERN_ELSEIF.IsMatch(line)))
                {
                    if (elseCaseAlreadyFound)
                    {
                        throw new ParserException(result, "found #if or #elseif after #else already occured. #else must be the last case in if statements.");
                    }
                    Match match;
                    if (isElseIfMatch)
                    {
                        match = PATTERN_ELSEIF.Match(line);
                    }
                    else {
                        match = PATTERN_IF.Match(line);
                    }
                    Case @case = new Case();
                    @case.lineNr = GetLineNr();

                    Condition cond = new Condition
                    {
                        varname = match.Groups[GROUP_IF_VARNAME].Value,
                        value = match.Groups[GROUP_IF_VALUE].Value,
                        unequal = match.Groups[GROUP_IF_OPERATOR].Value.Equals("!="),
                        lineNr = GetLineNr()
                    };
                    @case.condition = cond;
                    Block block = new Block();
                    block.lineNr = GetLineNr();
                    ReadBlockContent(template, block);
                    @case.block = block;
                    result.cases.Add(@case);
                }
                else if (PATTERN_ELSE.IsMatch(line))
                {
                    if (elseCaseAlreadyFound)
                    {
                        throw new ParserException(result, "found dulicated #else. #else must be the last case in if statements.");
                    }
                    elseCaseAlreadyFound = true;
                    Case @case = new Case();
                    @case.lineNr = GetLineNr();
                    @case.condition = null; // null means "else case"
                    Block block = new Block();
                    block.lineNr = GetLineNr();
                    ReadBlockContent(template, block);
                    @case.block = block;
                    result.cases.Add(@case);
                }
            }
            return result;
        }

        private InsertBlock ReadInsertBlock()
        {
            InsertBlock result = new InsertBlock();
            result.lineNr = GetLineNr();

            string line = ReadLine();
            Match match = PATTERN_INSERT_BLOCK.Match(line);

            result.blockRef = new BlockRef { blockName = match.Groups[GROUP_INSERT_BLOCK_BLOCKREF].Value, lineNr = GetLineNr() };
            result.leadingSpaces = match.Groups[GROUP_INSERT_BLOCK_LEADING].Value;

            return result;
        }

        private ForeachWithBlock ReadForeachWithBlock()
        {
            ForeachWithBlock result = new ForeachWithBlock();
            result.lineNr = GetLineNr();

            string line = ReadLine();
            Match match = PATTERN_FOREACH_WITH_BLOCK.Match(line);

            result.blockRef = new BlockRef { blockName = match.Groups[GROUP_FOREACH_WITH_BLOCK_BLOCKREF].Value, lineNr = GetLineNr() };
            result.dataKey = match.Groups[GROUP_FOREACH_WITH_BLOCK_KEY].Value;

            return result;
        }

        private TextNode ReadText()
        {
            StringBuilder sb = new StringBuilder();
            while(true)
            {
                string line = PeekLine();
                if (PeekLine() == null || IsSymbol(line))
                {
                    break;
                }
                sb.AppendLine(ReadLine());
            }
            return new TextNode
            {
                text = sb.ToString(),
                lineNr = GetLineNr()
            };
        }

        private bool IsSymbol(string line)
        {
            foreach (Regex symbol in symbols)
            {
                if (symbol.IsMatch(line))
                {
                    return true;
                } 
            }
            return false;
        }

        private Block ReadBlock(Template template)
        {
            Block result = new Block();
            
            string line = ReadLine();
            Match match = PATTERN_BEGIN_BLOCK.Match(line);

            result.leadingSpaces = match.Groups[GROUP_BEGIN_BLOCK_LEADING].Value;
            result.name = match.Groups[GROUP_BEGIN_BLOCK_NAME].Value;

            ReadBlockContent(template, result);

            template.blocks.Add(result.name, result);

            return result;
        }
    }

}

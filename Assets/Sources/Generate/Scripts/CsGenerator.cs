//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Collections.Generic;
using System.Text;

namespace EMP.Cs
{
    public abstract class CsGenerator {
        protected abstract string GetTemplate();

        protected virtual TemplateParser GetTemplateParser() {
            return new DefaultTemplateParser(GetTemplate());
        }

        public string Generate(Dictionary<string, object> data)
        {
            TemplateParser parser = GetTemplateParser();
            Template template = parser.Parse();
            StringBuilder sb = new StringBuilder();
            GenerateCodeFromBlock(template, template, data, sb);
            return sb.ToString();
        }

        private void GenerateCodeFromBlock(Template template, Block block, Dictionary<string, object> data, StringBuilder sb)
        {
            foreach(ParseNode node in block.childNodes)
            {
                if (node is TextNode)
                {
                    TextNode tn = node as TextNode;
                    GenerateCodeFromTextNode(tn, data, sb);
                } else if (node is ForeachWithBlock)
                {
                    ForeachWithBlock fewb = node as ForeachWithBlock;
                    GenerateCodeFromForeachWithBlock(template, fewb, data[fewb.dataKey] as List<Dictionary<string, object>>, fewb.blockRef.Find(template), sb);
                }
                else if (node is InsertBlock)
                {
                    InsertBlock ib = node as InsertBlock;
                    GenerateCodeFromBlock(template, ib.blockRef.Find(template), data, sb);
                }
                else if (node is If)
                {
                    If @if = node as If;
                    GenerateCodeFromIfStatement(template, @if, data, sb);
                }
            }
        }

        private void GenerateCodeFromIfStatement(Template template, If @if, Dictionary<string, object> data, StringBuilder sb)
        {
            foreach(Case @case in @if.cases)
            {
                // if condition is null, we are in else case.
                if (@case.condition == null || @case.condition.IsTrue(data))
                {
                    GenerateCodeFromBlock(template, @case.block, data, sb);
                    break;
                }
            }
        }

        private void GenerateCodeFromTextNode(TextNode tn, Dictionary<string, object> data, StringBuilder sb)
        {
            string text = tn.text;
            foreach (KeyValuePair<string, object> date in data)
            {
                text = text.Replace("#" + date.Key + "#", date.Value == null ? "null" : date.Value.ToString());
            }
            sb.Append(text);
        }

        private void GenerateCodeFromForeachWithBlock(Template template, ForeachWithBlock fewb, List<Dictionary<string, object>> dataList, Block block, StringBuilder sb)
        {
            foreach(Dictionary<string, object> data in dataList)
            {
                GenerateCodeFromBlock(template, block, data, sb);
            }
        }
    }

}

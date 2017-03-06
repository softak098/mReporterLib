using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace mReporterLib
{

    enum LineTemplateItemType { Text, Value }

    class LineTemplateItem
    {
        public LineTemplateItemType Type { get; set; }
        public int Width { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// Internal index if item is a Value
        /// </summary>
        public int Index { get; set; }

        public override string ToString()
        {
            if (Type == LineTemplateItemType.Text) return string.Format("{0} \"{1}\" W:{2}", this.Type, Content, Width);
            return string.Format("{0} W:{1}", this.Type, Width);
        }
    }

    /// <summary>
    /// Parsed content of line template
    /// </summary>
    class LineTemplate
    {
        string _lineTemplate;
        List<LineTemplateItem> _items;
        Line _line;
        Regex _filter;

        /// <summary>
        /// Returns number of values on line
        /// </summary>
        public int ValueCount => _items.Count(r => r.Type == LineTemplateItemType.Value);

        public LineTemplate(Line line, string lineTemplate)
        {
            _lineTemplate = lineTemplate;
            _line = line;
            _filter = new Regex(@"[\x00-\x09\x0B-\x1F]+");
            Parse();
        }


        void Parse()
        {
            _items = new List<LineTemplateItem>();
            if (_lineTemplate == null) return;

            StringBuilder sb = new StringBuilder();
            int lastFPos = 0;
            int valueIndex = 0;
            for (int i = 0; i < _lineTemplate.Length; i++) {

                if (_lineTemplate[i] == '_') {

                    if (sb.Length > 0) {

                        _items.Add(new LineTemplateItem {
                            Type = LineTemplateItemType.Text,
                            Content = _filter.Replace(sb.ToString(), ""),
                            Width = i - lastFPos
                        });
                        sb.Clear();
                    }

                    lastFPos = i;
                    while (i < _lineTemplate.Length && _lineTemplate[i] == '_') i++;

                    _items.Add(new LineTemplateItem {
                        Type = LineTemplateItemType.Value,
                        Width = i - lastFPos,
                        Index = valueIndex++
                    });
                    lastFPos = i--;

                }
                else {
                    sb.Append(_lineTemplate[i]);

                }

            }

            if (sb.Length > 0) {

                _items.Add(new LineTemplateItem {
                    Type = LineTemplateItemType.Text,
                    Content = _filter.Replace(sb.ToString(), ""),
                    Width = _lineTemplate.Length - lastFPos
                });

            }


        }

        /// <summary>
        /// Formats result value based on parameters from GetData handler
        /// </summary>
        internal void Build(RenderContext context, LineElement lineBuilder, GetDataResult[] resultData)
        {
            Dictionary<int, List<string>> multilineValues = null;
            Action<int, List<string>> _AddMultiline = (index, data) => {

                if (multilineValues == null) multilineValues = new Dictionary<int, List<string>>();

                if (multilineValues.ContainsKey(index)) multilineValues[index].AddRange(data);
                else multilineValues.Add(index, data);

            };

            // first line
            for (int i = 0; i < _items.Count; i++) {

                var item = _items[i];
                if (item.Type == LineTemplateItemType.Text) lineBuilder.Append(item.Content);
                else {
                    string firstLineValue = "";
                    var valueData = resultData[item.Index];
                    if (valueData.Value == null) {
                        firstLineValue = new string('%', item.Width);

                    }
                    else {
                        bool firstLine = true;
                        string currentValue = _filter.Replace(valueData.Value, "");

                        foreach (var valueLine in currentValue.Split('\n')) {

                            if (firstLine) {
                                if (valueLine.Length > item.Width) {

                                    if (!valueData.WordWrap) {
                                        _AddMultiline(item.Index, new List<string> { valueLine });
                                        firstLineValue = valueLine.Substring(0, item.Width);

                                    }
                                    else {
                                        _AddMultiline(item.Index, WordWrap(valueLine, item.Width));
                                        firstLineValue = AlignText(multilineValues[item.Index][0], item.Width, valueData.Alignment);
                                    }
                                }
                                else {
                                    _AddMultiline(item.Index, new List<string> { valueLine });
                                    firstLineValue = AlignText(valueLine, item.Width, valueData.Alignment);
                                }

                            }
                            else {
                                // everything else move to multiline object
                                _AddMultiline(item.Index, WordWrap(valueLine, item.Width));
                            }
                            firstLine = false;
                        }
                    }

                    var styleInfo = context.Report.Dialect.FontStyle(valueData.Style);
                    var alignInfo = context.Report.Dialect.Align(_line.Alignment);

                    lineBuilder.Append(firstLineValue, alignInfo, styleInfo);

                    //lineBuilder.Append(firstLineValue.ApplyEscCode(alignInfo, styleInfo));
                }
            }

            // rest of lines
            if (multilineValues != null) {
                int maxLines = 0;
                foreach (var item in multilineValues) maxLines = Math.Max(maxLines, item.Value.Count);
                for (int line = 1; line < maxLines; line++) {
                    lineBuilder.AppendLine();

                    for (int i = 0; i < _items.Count; i++) {
                        string nextLineValue;
                        bool applyStyle = true;

                        var item = _items[i];
                        var valueData = resultData[item.Index];

                        if (item.Type == LineTemplateItemType.Text) {
                            if (_line.RepeatStaticItems) nextLineValue = item.Content;
                            else nextLineValue = new string(' ', item.Width);
                            applyStyle = false; // style can be applied to value items only
                        }
                        else {
                            if (!multilineValues.ContainsKey(item.Index) || multilineValues[item.Index].Count <= line) {
                                nextLineValue = new string(' ', item.Width);

                            }
                            else nextLineValue = AlignText(multilineValues[item.Index][line], item.Width, valueData.Alignment);
                        }

                        if (applyStyle) {
                            var styleInfo = context.Report.Dialect.FontStyle(valueData.Style);
                            lineBuilder.Append(nextLineValue, styleInfo);
                        }
                        else lineBuilder.Append(nextLineValue);
                    }
                }
            }

            var lineStyleInfo = context.Report.Dialect.FontStyle(_line.Style);
            var linePrintInfo = context.Report.Dialect.PrintStyle(_line.PrintStyle);
            var lineAlignInfo = context.Report.Dialect.Align(_line.Alignment);
            var lineFontType = context.Report.Dialect.FontType(_line.FontType);

            lineBuilder.Apply(lineFontType, lineAlignInfo, linePrintInfo, lineStyleInfo);

            /*
            foreach (var lineStr in lineBuilder.ToString().Split('\n')) {

                result.Add(
                    lineStr.Trim('\r', '\n').ApplyEscCode(lineAlignInfo, linePrintInfo, lineStyleInfo)
                    );
            }
            */
        }

        static List<string> WordWrap(string the_string, int width)
        {
            if (width < 1) return new List<string> { the_string };

            int pos = 0, eol = the_string.Length;
            List<string> result = new List<string>();

            do {
                int len = eol - pos;

                if (len > width)
                    len = BreakLine(the_string, pos, width);

                result.Add(the_string.Substring(pos, len));

                pos += len;

                // Trim whitespace following break
                while (pos < eol && Char.IsWhiteSpace(the_string[pos])) pos++;

            } while (eol > pos);

            return result;
        }

        static int BreakLine(string text, int pos, int max)
        {
            int i = max - 1;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i])) i--;
            if (i < 0) return max;
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i])) i--;
            return i + 1;
        }



        static string AlignText(string s, int width, Align alignment)
        {
            if (width <= 0)
                return s;

            if (alignment == Align.Left) {
                return s.PadRight(width); //   string.Concat(s, new string(' ', width - s.Length));
            }
            else if (alignment == Align.Right) {
                return s.PadLeft(width); // string.Concat(new string(' ', width - s.Length), s);
            }
            else if (alignment == Align.Center) {
                int l = (width - s.Length) / 2;
                string r = s.PadRight(width - l); //  string.Concat(new string(' ', l), s);
                return r.PadLeft(width); //  string.Concat(r, new string(' ', width - r.Length));
            }

            int middle = s.Length / 2;
            Dictionary<int, int> spaceOffsetsToParts = new Dictionary<int, int>();
            string[] parts = s.Split(' ');
            for (int partIndex = 0, offset = 0; partIndex < parts.Length; partIndex++) {
                spaceOffsetsToParts.Add(offset, partIndex);
                offset += parts[partIndex].Length + 1; // +1 to count space that was removed by Split
            }
            foreach (var pair in spaceOffsetsToParts.OrderBy(entry => Math.Abs(middle - entry.Key))) {
                width--;
                if (width < 0)
                    break;
                parts[pair.Value] += ' ';
            }
            return String.Join(" ", parts);
        }

    }

}

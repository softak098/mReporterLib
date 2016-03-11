using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Returns number of values on line
        /// </summary>
        public int ValueCount { get { return _items.Count(r => r.Type == LineTemplateItemType.Value); } }

        public LineTemplate(Line line, string lineTemplate)
        {
            _lineTemplate = lineTemplate;
            _line = line;
            Parse();
        }


        void Parse()
        {
            _items = new List<LineTemplateItem>();

            StringBuilder sb = new StringBuilder();
            int lastFPos = 0;
            int valueIndex = 0;
            for (int i = 0; i < _lineTemplate.Length; i++) {

                if (_lineTemplate[i] == '_') {

                    if (sb.Length > 0) {

                        _items.Add(new LineTemplateItem {
                            Type = LineTemplateItemType.Text,
                            Content = sb.ToString(),
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
                    Content = sb.ToString(),
                    Width = _lineTemplate.Length - lastFPos
                });

            }


        }

        /// <summary>
        /// Formats result value based on parameters from GetData handler
        /// </summary>
        public List<string> Format(RenderContext context, GetDataResult[] resultData)
        {
            List<string> result = new List<string>();
            StringBuilder lineBuilder = new StringBuilder();

            Dictionary<int, List<string>> multilineValues = null;
            // first line
            for (int i = 0; i < _items.Count; i++) {

                var item = _items[i];
                if (item.Type == LineTemplateItemType.Text) lineBuilder.Append(item.Content);
                else {
                    string firstLineValue = "";
                    var valueData = resultData[item.Index];
                    if (valueData.Value == null) {
                        firstLineValue = new string('?', item.Width);

                    }
                    else {
                        bool firstLine = true;
                        bool isMultiline = valueData.Value.Any(c => c == '\n');

                        if (isMultiline) {
                            multilineValues = new Dictionary<int, List<string>>();

                        }

                        foreach (var valueLine in valueData.Value.Split('\n')) {

                            if (firstLine) {
                                if (valueLine.Length > item.Width) {

                                    if (!valueData.WordWrap) firstLineValue = valueLine.Substring(0, item.Width);
                                    else {
                                        multilineValues.Add(item.Index, Split(valueLine, item.Width));

                                        firstLineValue = AlignText(multilineValues[item.Index][0], item.Width, valueData.Alignment);
                                    }
                                }
                                else {
                                    firstLineValue = AlignText(valueLine, item.Width, valueData.Alignment);
                                }

                            }
                            else {
                                // everything else move to multiline object
                                var multilineData = Split(valueLine, item.Width);

                                if (multilineValues.ContainsKey(item.Index)) multilineValues[item.Index].AddRange(multilineData);
                                else multilineValues.Add(item.Index, multilineData);
                            }
                            firstLine = false;
                        }


                    }

                    var styleInfo = context.Report.Dialect.FontStyle(valueData.Style);
                    if (styleInfo != null) lineBuilder.Append(styleInfo.Start);
                    lineBuilder.Append(firstLineValue);
                    if (styleInfo != null) lineBuilder.Append(styleInfo.End);
                }
            }

            // rest of lines
            if (multilineValues != null) {
                int maxLines = 0;
                foreach (var item in multilineValues) maxLines = Math.Max(maxLines, item.Value.Count);
                for (int line = 0; line < maxLines; line++) {
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
                            if (!multilineValues.ContainsKey(item.Index)) {
                                nextLineValue = new string(' ', item.Width);

                            }
                            else nextLineValue = AlignText(multilineValues[item.Index][line], item.Width, valueData.Alignment);
                        }

                        if (applyStyle) {
                            var styleInfo = context.Report.Dialect.FontStyle(valueData.Style);
                            lineBuilder.Append(styleInfo.ApplyEscCode(nextLineValue));
                        }
                        else lineBuilder.Append(nextLineValue);
                    }
                }
            }

            var lineStyleInfo = context.Report.Dialect.FontStyle(_line.Style);
            var linePrintInfo = context.Report.Dialect.PrintStyle(_line.PrintStyle);
            foreach (var lineStr in lineBuilder.ToString().Split('\n')) {

                result.Add(
                    lineStr.Trim('\r', '\n').ApplyEscCode(linePrintInfo,lineStyleInfo)
                    );
            }
            return result;
        }

        static List<string> Split(string s, int width)
        {
            var result = new List<string>();
            StringBuilder sbLine = new StringBuilder();
            foreach (var word in s.Split(' ')) {
                if (string.IsNullOrWhiteSpace(word) || word.Any(c => c < 33)) continue;
                int wordLen = word.Length + (sbLine.Length > 0 ? 1 : 0);
                if (wordLen + sbLine.Length > width) {
                    result.Add(sbLine.ToString());
                    sbLine.Clear();
                }
                if (sbLine.Length > 0) sbLine.Append(' ');
                sbLine.Append(word);
            }
            if (sbLine.Length > 0) {
                result.Add(sbLine.ToString());
            }
            return result;
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
                string r = s.PadRight(l); //  string.Concat(new string(' ', l), s);
                return r.PadLeft(width); //  string.Concat(r, new string(' ', width - r.Length));
            }

            Int32 middle = s.Length / 2;
            IDictionary<Int32, Int32> spaceOffsetsToParts = new Dictionary<Int32, Int32>();
            String[] parts = s.Split(' ');
            for (Int32 partIndex = 0, offset = 0; partIndex < parts.Length; partIndex++) {
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

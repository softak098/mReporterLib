using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    public class PageBuilder
    {
        const string PAGE_NUMBER_PLACEHOLDER = "$P";
        const string TOTAL_PAGE_NUMBER_PLACEHOLDER = "$T";

        Report _report;
        EscCode _ffSequence;

        MemoryStream _outputStream;
        public MemoryStream OutputStream => _outputStream;

        Encoding _textEncoding;

        /// <summary>
        /// Specifies page height in lines
        /// </summary>
        public int PageHeight { get; private set; }

        public int CurrentLine { get; set; }
        public int CurrentPage { get; set; }

        public PageBuilder(Report report, Encoding textEncoding, int pageHeight = 66)
        {
            _report = report;
            _ffSequence = _report.Dialect.FormFeed();

            PageHeight = pageHeight;
            CurrentLine = 1;
            CurrentPage = 1;

            _outputStream = new MemoryStream();
            _textEncoding = textEncoding;
            Reset();
        }

        /// <summary>
        /// Build report output starting on specified element
        /// </summary>
        public void Build(RenderContext context, OutputElement rootElement = null)
        {

            foreach (var el in context.GetElements(rootElement)) {

                if (el is LineElement) {

                    BuildLines(context, (LineElement)el);
                    Build(context, el);

                }
                else {

                    el.WriteTo(_outputStream, _textEncoding);

                }

            }
        }



        void BuildLines(RenderContext context, LineElement el)
        {
            int linesToBuild = el.LineCount;
            if (linesToBuild == 0) return; // nothing to generate

            if (PageHeight == 0) {
                // in infinite report (for POS) there is no need to check paging
                el.WriteTo(_outputStream, _textEncoding);
                //AddToOutput(el, 0);
                return;
            }

            // check, if there is a page footer
            LineElement footerLine = GetFooterLine();
            int footerLines = footerLine != null ? footerLine.Lines.Count : 0;

            int startLine = 0;
            while (linesToBuild > 0) {
                bool pageBreak = CurrentLine + linesToBuild + footerLines > PageHeight + 1;
                if (pageBreak && el.SourceReportItem is Line) {
                    bool breakInside = (el.SourceReportItem as Line).PageBreakInside;
                    if (!breakInside) {
                        AddNewPage(context, true);
                        pageBreak = false;
                    }
                }

                int linesToInsert = PageHeight - footerLines - CurrentLine + 1;
                if (linesToInsert > linesToBuild) linesToInsert = linesToBuild;

                if (linesToInsert > 0) {
                    AddToOutput(el, startLine, linesToInsert);
                    startLine += linesToInsert;
                }

                if (pageBreak) {
                    // we should form new page
                    AddNewPage(context, true);
                }
                linesToBuild -= linesToInsert;
                CurrentLine += linesToInsert;
            }

        }

        /// <summary>
        /// Invokes printer reset operation
        /// </summary>
        internal void Reset()
        {
            _report.Dialect.Reset().WriteTo(_outputStream, _textEncoding);
            //_output.Append(_report.Dialect.Reset().Start); // we start with reseting printer
        }

        void AddNewPage(RenderContext context, bool addNextLines)
        {
            /*
            // insert page footer
            LineElement footerLine = GetFooterLine();
            int footerLines = footerLine != null ? footerLine.Lines.Count : 0;
            if (footerLine != null) {
                // add several empty lines to place footer on bottom of page
                for (int i = 0; i < PageHeight - footerLines - CurrentLine + 1; i++) {

                    _output.AppendLine();
                }
                // place footer line(s) to output
                AddToOutput(footerLine.Lines, 0); // !!! - do not replace with BuildInternal
            }
            // first replace page number placeholders in current output
            ReplacePageNumber();
            // insert new page control char
            _output.Append(_ffSequence.Start);

            if (addNextLines) {
                // update counters
                CurrentPage++;
                CurrentLine = 1;
                // add page header to new page, if any
                var headerLine = GetHeaderLine();
                if (headerLine != null) BuildLines(context, headerLine);

                // repeat item(s) marked as RepeatOnNewPage
                RepeatItemsOnNewPage(context);
            }
            */
        }

        void RepeatItemsOnNewPage(RenderContext context, LineElement rootLine = null)
        {
            /*
            foreach (var oLine in context.GetLines(rootLine)) {
                var lineItem = oLine.SourceReportItem as Line;

                if (lineItem != null && lineItem.RepeatOnNewPage) {

                    BuildInternal(context, oLine);

                }

                RepeatItemsOnNewPage(context, oLine);
            }
            */
        }

        #region header & footer lines
        LineElement _footerLine = null;
        bool _footerLineExists = true;
        LineElement GetFooterLine()
        {
            if (!_footerLineExists) return null;
            if (_footerLine != null) return _footerLine;
            var footer = _report.GetPageFooter();
            if (footer != null) {
                RenderContext footerContext = new RenderContext(_report);
                footer.Render(footerContext);
                _footerLine = footerContext.LastLineElement;
            }
            return _footerLine;
        }

        LineElement _headerLine = null;
        bool _headerLineExists = true;
        LineElement GetHeaderLine()
        {
            if (!_headerLineExists) return null;
            if (_headerLine != null) return _headerLine;
            var header = _report.GetPageHeader();
            if (header != null) {
                RenderContext headerContext = new RenderContext(_report);
                header.Render(headerContext);
                _headerLine = headerContext.LastLineElement;
            }
            return _headerLine;
        }

        internal void FinishReport(RenderContext context)
        {
            AddNewPage(context, false);
        }

        #endregion


        internal void ReplacePageNumber()
        {
            //_output.Replace(PAGE_NUMBER_PLACEHOLDER, CurrentPage.ToString().PadLeft(2));
        }

        internal void ReplaceTotalPageNumber()
        {
            //_output.Replace(TOTAL_PAGE_NUMBER_PLACEHOLDER, CurrentPage.ToString().PadLeft(2));
        }

        void AddToOutput(LineElement line, int from, int count = int.MaxValue, bool addNewLine = true)
        {

            count = Math.Min(count, line.LineCount - from);

            line.WriteTo(_outputStream, _textEncoding, from, count);
            /*
            for (int i = 0; i < count; i++) {


                line.Lines[i + from].

                /*
                if (addNewLine) {
                    _output.AppendLine(lines[i + from]);
                }
                else {
                    _output.Append(lines[i + from]);
                }
            }
            */


        }


        /// <summary>
        /// Final output from generator
        /// </summary>
        public string Output
        {
            get { return ""; } 
        }

    }
}

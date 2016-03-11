using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    public class PageBuilder
    {
        const string PAGE_NUMBER_PLACEHOLDER = "$P";
        const string TOTAL_PAGE_NUMBER_PLACEHOLDER = "$T";

        StringBuilder _output;
        Report _report;
        Sequence _ffSequence;

        /// <summary>
        /// Specifies page height in lines
        /// </summary>
        public int PageHeight { get; private set; }

        public int CurrentLine { get; set; }
        public int CurrentPage { get; set; }

        public PageBuilder(Report report, int pageHeight = 66)
        {
            _report = report;
            _ffSequence = _report.Dialect.FormFeed();

            PageHeight = pageHeight;
            CurrentLine = 1;
            CurrentPage = 1;
            _output = new StringBuilder();
            _output.Append(_report.Dialect.Reset().Start); // we start with reseting printer
        }

        /// <summary>
        /// Build report output starting on specified output line
        /// </summary>
        public void Build(RenderContext context, OutputLine rootLine = null)
        {

            // we start from parent output lines
            foreach (var oLine in context.GetLines(rootLine)) {

                BuildInternal(context, oLine);
                Build(context, oLine);

            }
        }



        void BuildInternal(RenderContext context, OutputLine lineObj)
        {
            int linesToBuild = lineObj.GeneratedLines.Count;
            if (linesToBuild == 0) return; // nothing to generate

            if (PageHeight == 0) {
                // in infinite report (for POS) there is no need to check paging
                AddToOutput(lineObj.GeneratedLines, 0);
                return;
            }

            // check, if there is a page footer
            OutputLine footerLine = GetFooterLine();
            int footerLines = footerLine != null ? footerLine.GeneratedLines.Count : 0;

            int startLine = 0;
            while (linesToBuild > 0) {
                bool nextPage = CurrentLine + linesToBuild + footerLines > PageHeight + 1;

                int linesToInsert = PageHeight - footerLines - CurrentLine + 1;
                if (linesToInsert > linesToBuild) linesToInsert = linesToBuild;

                if (linesToInsert > 0) {
                    AddToOutput(lineObj.GeneratedLines, startLine, linesToInsert);
                    startLine += linesToInsert;
                }

                if (nextPage) {
                    // we should form new page
                    AddNewPage(context, true);
                }
                linesToBuild -= linesToInsert;
                CurrentLine += linesToInsert;
            }

        }


        void AddNewPage(RenderContext context, bool addNextLines)
        {
            // insert page footer
            OutputLine footerLine = GetFooterLine();
            int footerLines = footerLine != null ? footerLine.GeneratedLines.Count : 0;
            if (footerLine != null) {
                // add several empty lines to place footer on bottom of page
                for (int i = 0; i < PageHeight - footerLines - CurrentLine+1; i++) {

                    _output.AppendLine();
                }
                // place footer line(s) to output
                AddToOutput(footerLine.GeneratedLines, 0); // !!! - do not replace with BuildInternal
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
                if (headerLine != null) BuildInternal(context, headerLine);

                // repeat item(s) marked as RepeatOnNewPage
                RepeatItemsOnNewPage(context);
            }
        }

        void RepeatItemsOnNewPage(RenderContext context, OutputLine rootLine = null)
        {
            foreach (var oLine in context.GetLines(rootLine)) {
                var lineItem = oLine.SourceReportItem as Line;

                if (lineItem != null && lineItem.RepeatOnNewPage) {

                    BuildInternal(context, oLine);

                }

                RepeatItemsOnNewPage(context, oLine);
            }
        }

        #region header & footer lines
        OutputLine _footerLine = null;
        bool _footerLineExists = true;
        OutputLine GetFooterLine()
        {
            if (!_footerLineExists) return null;
            if (_footerLine != null) return _footerLine;
            var footer = _report.GetPageFooter();
            if (footer != null) {
                RenderContext footerContext = new RenderContext(_report);
                _footerLine = footer.Render(footerContext);
            }
            return _footerLine;
        }

        OutputLine _headerLine = null;
        bool _headerLineExists = true;
        OutputLine GetHeaderLine()
        {
            if (!_headerLineExists) return null;
            if (_headerLine != null) return _headerLine;
            var header = _report.GetPageHeader();
            if (header != null) {
                RenderContext headerContext = new RenderContext(_report);
                _headerLine = header.Render(headerContext);
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
            _output.Replace(PAGE_NUMBER_PLACEHOLDER, CurrentPage.ToString().PadLeft(2));
        }

        internal void ReplaceTotalPageNumber()
        {
            _output.Replace(TOTAL_PAGE_NUMBER_PLACEHOLDER, CurrentPage.ToString().PadLeft(2));
        }

        void AddToOutput(List<string> lines, int from, int count = int.MaxValue)
        {
            count = Math.Min(count, lines.Count - from);
            for (int i = 0; i < count; i++) {
                _output.AppendLine(lines[i + from]);
            }

        }


        /// <summary>
        /// Final output from generator
        /// </summary>
        public string Output
        {
            get
            { return _output.ToString(); }
        }

    }
}

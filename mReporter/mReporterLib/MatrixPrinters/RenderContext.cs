using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class RenderContext
    {
        internal Report Report { get; private set; }

        public RenderContext(Report report)
        {
            this.Report = report;
            //_currentOutputLine = null;
            //_outputLines = new List<LineElement>();

            _elements = new List<OutputElement>();
        }

        //LineElement _currentOutputLine;
        //internal LineElement CurrentOutputParent => _currentOutputLine;
        //List<LineElement> _outputLines;
        //internal List<LineElement> OutputLines => _outputLines;

        internal LineElement LastLineElement
        {
            get
            {
                return _elements.Where(r => r is LineElement).Last() as LineElement;
            }
        }


        OutputElement _parentElement;
        List<OutputElement> _elements;

        /*
        internal IEnumerable<LineElement> GetLines(LineElement parent)
        {
            foreach (var oLine in _outputLines.Where(r => r.Parent == parent)) {
                yield return oLine;
            }
        }
        */

        internal IEnumerable<OutputElement> GetElements(OutputElement parentElement)
        {
            foreach (var oLine in _elements.Where(r => r.Parent == parentElement)) {
                yield return oLine;
            }
        }


        internal void AddToOutput(ReportItem item, OutputElement element)
        {
            element.SourceReportItem = item;
            element.Parent = _parentElement;
            _elements.Add(element);
        }

        internal void SetParentElement(OutputElement parentElement)
        {
            _parentElement = parentElement;
        }

    }

}

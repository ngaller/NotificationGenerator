using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using SSSWorld.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;
using SSSWorld.RFI.NotificationGenerator.Shared;


namespace SSSWorld.RFI.NotificationGenerator
{
    public class MergePDF
    {
        private readonly Configuration _configuration;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MergePDF));

        public MergePDF(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Append source pdf at the end of target pdf.
        /// If target does not exist, copy source to target path.
        /// If there is any error in the merge, leave target untouched (as much as possible) and return inPages
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="inPages">Number of pages in target.  If there is an error in the merge this will be used as return value (rather than recounting the pages)</param>
        /// <returns>total number of pages in the pdf</returns>
        public int AppendToPdf(string target, string source, int inPages)
        {
            return AppendOrPrepend(target, source, inPages, false);
        }

        /// <summary>
        /// Insert source PDF at the beginning of target.
        /// If target does not exist, copy source onto target path
        /// If there is any error in the merge, leave target untouched (as much as possible) and return 0
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="inPages">Number of pages in target.  If there is an error in the merge this will be used as return value (rather than recounting the pages)</param>
        /// <returns>total number of pages in the pdf</returns>
        public int PrependToPdf(string target, string source, int inPages)
        {
            return AppendOrPrepend(target, source, inPages, true);
        }

        private int AppendOrPrepend(string target, string source, int inPages, bool prepend)
        {
            if (!File.Exists(source))
            {
                LOG.Warn($"Attempted to merge {source} into {target} but it does not exist!");
                return inPages;
            }
            try
            {
                if (File.Exists(target))
                {
                    var temp = Utils.GetUniqueFileName(_configuration.TemporaryFolder, "mergePdf", ".pdf");
                    string first = prepend ? source : target, second = prepend ? target : source;
                    if (MergeTwoPDFs(temp, first, second, ref inPages))
                    {
                        File.Delete(target);
                        File.Move(temp, target);
                        return inPages;
                    }
                }
                else
                {
                    File.Copy(source, target);
                    return CountPagesInPdf(target);
                }
            }
            catch (Exception x)
            {
                LOG.Warn($"Error merging {target} and {source} - skipping", x);
            }
            return inPages;
        }


        /// <summary>
        /// Merge 2 pdf into outputFile.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="inputFile1"></param>
        /// <param name="inputFile2"></param>
        /// <param name="numberOfPages">Will be set to total number of pages in the pdf.  If the merge fails this will be left unmodified</param>
        /// <returns></returns>
        static public bool MergeTwoPDFs(string outputFile, string inputFile1, string inputFile2, ref int numberOfPages)
        {
            try
            {
                if (!File.Exists(inputFile1))
                    throw new ArgumentException($"Trying to merge non existent path ${inputFile1}", nameof(inputFile1));
                if (!File.Exists(inputFile2))
                    throw new ArgumentException($"Trying to merge non existent path ${inputFile2}", nameof(inputFile2));
                if (inputFile2 == outputFile || inputFile1 == outputFile)
                    throw new ArgumentException("When merging output path must be different from input", nameof(outputFile));
                if (!inputFile1.ToLower().EndsWith("pdf") || !inputFile2.ToLower().EndsWith("pdf"))
                {
                    LOG.Warn("Cannot merge attachments: 1 of the file is not PDF.  " + inputFile1 + ", " + inputFile2);
                    return false; //Can't do this merge. 
                }

                using (var reader1 = new PdfReader(inputFile1))
                using (var reader2 = new PdfReader(inputFile2))
                using (var stream = new FileStream(outputFile, FileMode.Create))
                {
                    using (Document doc = new Document(reader1.GetPageSizeWithRotation(1)))
                    {
                        PdfCopy pdf = new PdfCopy(doc, stream);
                        doc.Open();
                        pdf.AddDocument(reader1);
                        pdf.AddDocument(reader2);
                        numberOfPages = reader1.NumberOfPages + reader2.NumberOfPages;
                    }
                }
                //                PdfReader reader = new PdfReader(inputFile1);
                //                using (Document doc = new Document(reader.GetPageSizeWithRotation(1)))
                //                using (PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outputFile, FileMode.Create)))
                //                {
                //                    doc.Open();
                //
                //                    AppendToDoc(reader, writer, doc);
                //
                //                    reader.Close();
                //
                //
                //                    reader = new PdfReader(inputFile2);
                //
                //                    AppendToDoc(reader, writer, doc);
                //
                //                    doc.Close();
                //                    reader.Close();
                //                    writer.Close();
                //                }
            }
            catch (Exception x)
            {
                LOG.Warn("MergeTwoPDFs failed - skipping attachment (output = " + outputFile + ", input 1 = " + inputFile1 + ", input 2 = " + inputFile2 + ")", x);
                return false;
            }

            return true;
        }

        public static int CountPagesInPdf(String file)
        {
            using (PdfReader reader = new PdfReader(file))
            {
                return reader.NumberOfPages;
            }
        }


        static private void AppendToDoc(PdfReader reader, PdfWriter writer, Document doc)
        {
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                doc.SetPageSize(reader.GetPageSizeWithRotation(i));
                doc.NewPage();
                page = writer.GetImportedPage(reader, i);
                rotation = reader.GetPageRotation(i);
                cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
            }
        }

    }
}

namespace ImageService
{
    partial class ImageService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.IS_eventLogger = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.IS_eventLogger)).BeginInit();
            // 
            // IS_eventLogger
            // 
            this.IS_eventLogger.EntryWritten += new System.Diagnostics.EntryWrittenEventHandler(this.IS_eventLogger_EntryWritten);
            // 
            // ImageService
            // 
            this.ServiceName = "ImageService";
            ((System.ComponentModel.ISupportInitialize)(this.IS_eventLogger)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog IS_eventLogger;
    }
}

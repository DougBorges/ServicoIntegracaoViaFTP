using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ServicoIntegracaoViaFtp.Executor {
    partial class FormIntegracaoViaFtp {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(Boolean disposing) {
            if (disposing && components != null) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            labelMensagem = new Label();
            SuspendLayout();
            // 
            // labelMensagem
            // 
            labelMensagem.AutoSize = true;
            labelMensagem.Location = new Point(12, 17);
            labelMensagem.Name = "labelMensagem";
            labelMensagem.Size = new Size(288, 13);
            labelMensagem.TabIndex = 0;
            labelMensagem.Text = "Processando dados para integração...";
            labelMensagem.UseWaitCursor = true;
            // 
            // FormIntegracaoViaFtp
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(337, 50);
            Controls.Add(labelMensagem);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormIntegracaoViaFtp";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Integração dos Dados Via FTP";
            UseWaitCursor = true;
            Shown += new EventHandler(FormIntegracaoViaFtp_Shown);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelMensagem;
    }
}


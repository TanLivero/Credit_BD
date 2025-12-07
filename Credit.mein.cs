// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.mein
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Credit;

public class mein : Form
{
	private IContainer components = null;

	public mein()
	{
		InitializeComponent();
		SetupForm();
	}

	private void SetupForm()
	{
		Text = "Система управления кредитной базой данных";
		base.Size = new Size(500, 400);
		base.StartPosition = FormStartPosition.CenterScreen;
		TableLayoutPanel tableLayout = new TableLayoutPanel();
		tableLayout.Dock = DockStyle.Fill;
		tableLayout.ColumnCount = 2;
		tableLayout.RowCount = 3;
		base.Controls.Add(tableLayout);
		Button[] buttons = new Button[6];
		string[] buttonTexts = new string[6] { "Клиенты", "Магазины", "Сотрудники", "Товары", "Кредитные договоры", "Платежи" };
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i] = new Button();
			buttons[i].Text = buttonTexts[i];
			buttons[i].Dock = DockStyle.Fill;
			buttons[i].Margin = new Padding(10);
			buttons[i].Font = new Font("Arial", 12f);
			buttons[i].Click += OpenFormHandler;
			tableLayout.Controls.Add(buttons[i]);
		}
	}

	private void OpenFormHandler(object sender, EventArgs e)
	{
		Button button = (Button)sender;
		Form form = null;
		switch (button.Text)
		{
		case "Клиенты":
			form = new ClientsForm();
			break;
		case "Магазины":
			form = new ShopsForm();
			break;
		case "Сотрудники":
			form = new EmployeesForm();
			break;
		case "Товары":
			form = new ProductsForm();
			break;
		case "Кредитные договоры":
			form = new ContractsForm();
			break;
		case "Платежи":
			form = new PaymentsForm();
			break;
		}
		form?.ShowDialog();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Name = "mein";
		this.Text = "main";
		base.ResumeLayout(false);
	}
}

// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.ClientsForm
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Credit;
using Npgsql;

public class ClientsForm : Form
{
	private DatabaseHelper dbHelper = null;

	private DataTable dataTable = null;

	private IContainer components = null;

	private Button btnUpdate;

	private Button btnDelete;

	private DataGridView dataGridViewClients;

	private TextBox txtFirstName;

	private Label label1;

	private TextBox txtLastName;

	private Label label2;

	private Label label3;

	private TextBox txtAddress;

	private TextBox txtPatronymic;

	private TextBox txtPassportSeries;

	private TextBox txtPassportNumber;

	private TextBox txtPhone;

	private TextBox txtEmail;

	private Button btnAdd;

	private Label label4;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	public ClientsForm()
	{
		InitializeComponent();
		dbHelper = new DatabaseHelper();
		LoadData();
		SetupForm();
		btnAdd.Click += btnAdd_Click;
		btnUpdate.Click += btnUpdate_Click;
		btnDelete.Click += btnDelete_Click;
		dataGridViewClients.SelectionChanged += dataGridViewClients_SelectionChanged;
	}

	private void SetupForm()
	{
		dataGridViewClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		dataGridViewClients.ReadOnly = true;
		dataGridViewClients.MultiSelect = false;
		dataGridViewClients.AllowUserToAddRows = false;
		dataGridViewClients.AllowUserToDeleteRows = false;
		btnUpdate.Enabled = false;
		btnDelete.Enabled = false;
	}

	private void LoadData()
	{
		try
		{
			string query = "SELECT * FROM Clients ORDER BY client_id";
			dataTable = dbHelper.ExecuteQuery(query);
			dataGridViewClients.DataSource = dataTable;
			if (dataGridViewClients.Columns.Contains("first_name"))
			{
				dataGridViewClients.Columns["first_name"].HeaderText = "Имя";
			}
			if (dataGridViewClients.Columns.Contains("last_name"))
			{
				dataGridViewClients.Columns["last_name"].HeaderText = "Фамилия";
			}
			if (dataGridViewClients.Columns.Contains("patronymic"))
			{
				dataGridViewClients.Columns["patronymic"].HeaderText = "Отчество";
			}
			if (dataGridViewClients.Columns.Contains("passport_series"))
			{
				dataGridViewClients.Columns["passport_series"].HeaderText = "Серия паспорта";
			}
			if (dataGridViewClients.Columns.Contains("passport_number"))
			{
				dataGridViewClients.Columns["passport_number"].HeaderText = "Номер паспорта";
			}
			if (dataGridViewClients.Columns.Contains("phone_number"))
			{
				dataGridViewClients.Columns["phone_number"].HeaderText = "Телефон";
			}
			if (dataGridViewClients.Columns.Contains("email"))
			{
				dataGridViewClients.Columns["email"].HeaderText = "Email";
			}
			if (dataGridViewClients.Columns.Contains("registration_address"))
			{
				dataGridViewClients.Columns["registration_address"].HeaderText = "Адрес";
			}
			if (dataGridViewClients.Columns.Contains("client_id"))
			{
				dataGridViewClients.Columns["client_id"].Visible = false;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void dataGridViewClients_SelectionChanged(object sender, EventArgs e)
	{
		bool hasSelection = dataGridViewClients.CurrentRow != null && dataGridViewClients.CurrentRow.Index >= 0;
		btnUpdate.Enabled = hasSelection;
		btnDelete.Enabled = hasSelection;
		if (hasSelection)
		{
			DataGridViewRow row = dataGridViewClients.CurrentRow;
			txtFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
			txtLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";
			txtPatronymic.Text = row.Cells["patronymic"].Value?.ToString() ?? "";
			txtPassportSeries.Text = row.Cells["passport_series"].Value?.ToString() ?? "";
			txtPassportNumber.Text = row.Cells["passport_number"].Value?.ToString() ?? "";
			txtPhone.Text = row.Cells["phone_number"].Value?.ToString() ?? "";
			txtEmail.Text = row.Cells["email"].Value?.ToString() ?? "";
			txtAddress.Text = row.Cells["registration_address"].Value?.ToString() ?? "";
		}
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		try
		{
			if (ValidateRequiredFields() && ValidatePassportUnique(txtPassportSeries.Text, txtPassportNumber.Text))
			{
				string query = "INSERT INTO Clients \r\n                (first_name, last_name, patronymic, passport_series, passport_number, \r\n                 phone_number, email, registration_address) \r\n                 VALUES (@firstName, @lastName, @patronymic, @passportSeries, \r\n                 @passportNumber, @phone, @email, @address)";
				NpgsqlParameter[] parameters = new NpgsqlParameter[8]
				{
					new NpgsqlParameter("@firstName", txtFirstName.Text.Trim()),
					new NpgsqlParameter("@lastName", txtLastName.Text.Trim()),
					new NpgsqlParameter("@patronymic", ((object)txtPatronymic.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@passportSeries", ((object)txtPassportSeries.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@passportNumber", ((object)txtPassportNumber.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@phone", ((object)txtPhone.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@email", ((object)txtEmail.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@address", ((object)txtAddress.Text.Trim()) ?? ((object)DBNull.Value))
				};
				int result = dbHelper.ExecuteNonQuery(query, parameters);
				if (result > 0)
				{
					LoadData();
					MessageBox.Show("Клиент добавлен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось добавить клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при добавлении клиента: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		if (dataGridViewClients.CurrentRow != null)
		{
			try
			{
				if (ValidateRequiredFields())
				{
					int clientId = Convert.ToInt32(dataGridViewClients.CurrentRow.Cells["client_id"].Value);
					if (ValidatePassportUnique(txtPassportSeries.Text, txtPassportNumber.Text, clientId))
					{
						string query = "UPDATE Clients SET \r\n                    first_name=@firstName, last_name=@lastName, \r\n                    patronymic=@patronymic, passport_series=@passportSeries, \r\n                    passport_number=@passportNumber, phone_number=@phone, \r\n                    email=@email, registration_address=@address \r\n                    WHERE client_id=@clientId";
						NpgsqlParameter[] parameters = new NpgsqlParameter[9]
						{
							new NpgsqlParameter("@firstName", txtFirstName.Text.Trim()),
							new NpgsqlParameter("@lastName", txtLastName.Text.Trim()),
							new NpgsqlParameter("@patronymic", ((object)txtPatronymic.Text.Trim()) ?? ((object)DBNull.Value)),
							new NpgsqlParameter("@passportSeries", ((object)txtPassportSeries.Text.Trim()) ?? ((object)DBNull.Value)),
							new NpgsqlParameter("@passportNumber", ((object)txtPassportNumber.Text.Trim()) ?? ((object)DBNull.Value)),
							new NpgsqlParameter("@phone", ((object)txtPhone.Text.Trim()) ?? ((object)DBNull.Value)),
							new NpgsqlParameter("@email", ((object)txtEmail.Text.Trim()) ?? ((object)DBNull.Value)),
							new NpgsqlParameter("@address", ((object)txtAddress.Text.Trim()) ?? ((object)DBNull.Value)),
							new NpgsqlParameter("@clientId", clientId)
						};
						int result = dbHelper.ExecuteNonQuery(query, parameters);
						if (result > 0)
						{
							LoadData();
							MessageBox.Show("Данные обновлены успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						}
						else
						{
							MessageBox.Show("Не удалось обновить данные!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
					}
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите клиента для обновления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void btnDelete_Click(object sender, EventArgs e)
	{
		if (dataGridViewClients.CurrentRow != null)
		{
			DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранного клиента?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				return;
			}
			try
			{
				int clientId = Convert.ToInt32(dataGridViewClients.CurrentRow.Cells["client_id"].Value);
				string query = "DELETE FROM Clients WHERE client_id=@clientId";
				NpgsqlParameter[] parameters = new NpgsqlParameter[1]
				{
					new NpgsqlParameter("@clientId", clientId)
				};
				int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);
				if (rowsAffected > 0)
				{
					LoadData();
					MessageBox.Show("Клиент удален успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось удалить клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при удалении клиента: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите клиента для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private bool ValidatePassportUnique(string series, string number, int excludeClientId = -1)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(series) && string.IsNullOrWhiteSpace(number))
			{
				return true;
			}
			string query = "SELECT COUNT(*) FROM Clients WHERE passport_series = @series AND passport_number = @number";
			List<NpgsqlParameter> parameters = new List<NpgsqlParameter>
			{
				new NpgsqlParameter("@series", ((object)series) ?? ((object)DBNull.Value)),
				new NpgsqlParameter("@number", ((object)number) ?? ((object)DBNull.Value))
			};
			if (excludeClientId > 0)
			{
				query += " AND client_id != @excludeId";
				parameters.Add(new NpgsqlParameter("@excludeId", excludeClientId));
			}
			object result = dbHelper.ExecuteScalar(query, parameters.ToArray());
			int count = ((result != DBNull.Value) ? Convert.ToInt32(result) : 0);
			if (count > 0)
			{
				MessageBox.Show("Клиент с такими паспортными данными уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка проверки паспортных данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
	}

	private bool ValidateRequiredFields()
	{
		if (string.IsNullOrWhiteSpace(txtFirstName.Text))
		{
			MessageBox.Show("Имя обязательно для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtFirstName.Focus();
			return false;
		}
		if (string.IsNullOrWhiteSpace(txtLastName.Text))
		{
			MessageBox.Show("Фамилия обязательна для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtLastName.Focus();
			return false;
		}
		return true;
	}

	private void ClearFields()
	{
		txtFirstName.Clear();
		txtLastName.Clear();
		txtPatronymic.Clear();
		txtPassportSeries.Clear();
		txtPassportNumber.Clear();
		txtPhone.Clear();
		txtEmail.Clear();
		txtAddress.Clear();
		dataGridViewClients.ClearSelection();
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
		this.btnUpdate = new System.Windows.Forms.Button();
		this.btnDelete = new System.Windows.Forms.Button();
		this.dataGridViewClients = new System.Windows.Forms.DataGridView();
		this.txtFirstName = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtLastName = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.txtAddress = new System.Windows.Forms.TextBox();
		this.txtPatronymic = new System.Windows.Forms.TextBox();
		this.txtPassportSeries = new System.Windows.Forms.TextBox();
		this.txtPassportNumber = new System.Windows.Forms.TextBox();
		this.txtPhone = new System.Windows.Forms.TextBox();
		this.txtEmail = new System.Windows.Forms.TextBox();
		this.btnAdd = new System.Windows.Forms.Button();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewClients).BeginInit();
		base.SuspendLayout();
		this.btnUpdate.Location = new System.Drawing.Point(741, 393);
		this.btnUpdate.Name = "btnUpdate";
		this.btnUpdate.Size = new System.Drawing.Size(89, 45);
		this.btnUpdate.TabIndex = 0;
		this.btnUpdate.Text = "Обновить";
		this.btnUpdate.UseVisualStyleBackColor = true;
		this.btnDelete.Location = new System.Drawing.Point(836, 393);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(93, 45);
		this.btnDelete.TabIndex = 1;
		this.btnDelete.Text = "Удалить";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.dataGridViewClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewClients.Location = new System.Drawing.Point(1, 72);
		this.dataGridViewClients.Name = "dataGridViewClients";
		this.dataGridViewClients.Size = new System.Drawing.Size(928, 320);
		this.dataGridViewClients.TabIndex = 2;
		this.txtFirstName.Location = new System.Drawing.Point(1, 27);
		this.txtFirstName.Multiline = true;
		this.txtFirstName.Name = "txtFirstName";
		this.txtFirstName.Size = new System.Drawing.Size(100, 28);
		this.txtFirstName.TabIndex = 3;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(29, 8);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(31, 15);
		this.label1.TabIndex = 4;
		this.label1.Text = "Имя";
		this.txtLastName.Location = new System.Drawing.Point(107, 27);
		this.txtLastName.Multiline = true;
		this.txtLastName.Name = "txtLastName";
		this.txtLastName.Size = new System.Drawing.Size(100, 28);
		this.txtLastName.TabIndex = 3;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(125, 8);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(58, 15);
		this.label2.TabIndex = 5;
		this.label2.Text = "Фамилия";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(235, 8);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(58, 15);
		this.label3.TabIndex = 6;
		this.label3.Text = "Отчество";
		this.txtAddress.Location = new System.Drawing.Point(741, 27);
		this.txtAddress.Multiline = true;
		this.txtAddress.Name = "txtAddress";
		this.txtAddress.Size = new System.Drawing.Size(100, 28);
		this.txtAddress.TabIndex = 3;
		this.txtPatronymic.Location = new System.Drawing.Point(213, 26);
		this.txtPatronymic.Multiline = true;
		this.txtPatronymic.Name = "txtPatronymic";
		this.txtPatronymic.Size = new System.Drawing.Size(100, 28);
		this.txtPatronymic.TabIndex = 3;
		this.txtPassportSeries.Location = new System.Drawing.Point(319, 27);
		this.txtPassportSeries.Multiline = true;
		this.txtPassportSeries.Name = "txtPassportSeries";
		this.txtPassportSeries.Size = new System.Drawing.Size(100, 28);
		this.txtPassportSeries.TabIndex = 3;
		this.txtPassportNumber.Location = new System.Drawing.Point(425, 27);
		this.txtPassportNumber.Multiline = true;
		this.txtPassportNumber.Name = "txtPassportNumber";
		this.txtPassportNumber.Size = new System.Drawing.Size(100, 28);
		this.txtPassportNumber.TabIndex = 3;
		this.txtPhone.Location = new System.Drawing.Point(531, 27);
		this.txtPhone.Multiline = true;
		this.txtPhone.Name = "txtPhone";
		this.txtPhone.Size = new System.Drawing.Size(100, 28);
		this.txtPhone.TabIndex = 3;
		this.txtEmail.Location = new System.Drawing.Point(637, 27);
		this.txtEmail.Multiline = true;
		this.txtEmail.Name = "txtEmail";
		this.txtEmail.Size = new System.Drawing.Size(100, 28);
		this.txtEmail.TabIndex = 3;
		this.btnAdd.Location = new System.Drawing.Point(849, 27);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(80, 28);
		this.btnAdd.TabIndex = 7;
		this.btnAdd.Text = "Добавить";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(348, 8);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(41, 15);
		this.label4.TabIndex = 6;
		this.label4.Text = "Серия";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(450, 8);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(45, 15);
		this.label5.TabIndex = 6;
		this.label5.Text = "Номер";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(553, 8);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(56, 15);
		this.label6.TabIndex = 6;
		this.label6.Text = "Телефон";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(665, 8);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(43, 15);
		this.label7.TabIndex = 6;
		this.label7.Text = "Эмайл";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(770, 8);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(46, 15);
		this.label8.TabIndex = 6;
		this.label8.Text = "Адресс";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(932, 450);
		base.Controls.Add(this.btnAdd);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtEmail);
		base.Controls.Add(this.txtPhone);
		base.Controls.Add(this.txtPassportNumber);
		base.Controls.Add(this.txtPassportSeries);
		base.Controls.Add(this.txtPatronymic);
		base.Controls.Add(this.txtAddress);
		base.Controls.Add(this.txtLastName);
		base.Controls.Add(this.txtFirstName);
		base.Controls.Add(this.dataGridViewClients);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnUpdate);
		base.Name = "ClientsForm";
		this.Text = "Clients";
		((System.ComponentModel.ISupportInitialize)this.dataGridViewClients).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

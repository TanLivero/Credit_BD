// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.PaymentsForm
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Credit;
using Npgsql;

public class PaymentsForm : Form
{
	private DatabaseHelper dbHelper;

	private DataTable dataTable;

	private DataTable contractsData;

	private IContainer components = null;

	private DataGridView dataGridViewPayments;

	private DateTimePicker dtpPaymentDate;

	private DateTimePicker dtpActualPaymentDate;

	private TextBox txtScheduledAmount;

	private TextBox txtPaidAmount;

	private TextBox txtPrincipalAmount;

	private TextBox txtInterestAmount;

	private ComboBox comboPaymentType;

	private Button btnRefreshContracts;

	private Button btnUpdate;

	private Button btnDelete;

	private ComboBox comboContractId;

	private Button btnRefreshData;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private Label label9;

	public PaymentsForm()
	{
		InitializeComponent();
		dbHelper = new DatabaseHelper();
		LoadPaymentTypes();
		LoadContracts();
		LoadData();
		SetupForm();
		btnRefreshContracts.Click += btnAdd_Click;
		btnUpdate.Click += btnUpdate_Click;
		btnDelete.Click += btnDelete_Click;
		btnRefreshContracts.Click += btnRefreshContracts_Click;
		dataGridViewPayments.SelectionChanged += dataGridViewPayments_SelectionChanged;
	}

	private void SetupForm()
	{
		dataGridViewPayments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		dataGridViewPayments.ReadOnly = true;
		dataGridViewPayments.MultiSelect = false;
		dataGridViewPayments.AllowUserToAddRows = false;
		dataGridViewPayments.AllowUserToDeleteRows = false;
		dtpPaymentDate.Value = DateTime.Now;
		dtpActualPaymentDate.Value = DateTime.Now;
		SetupContractsComboBox();
		btnUpdate.Enabled = false;
		btnDelete.Enabled = false;
	}

	private void LoadContracts()
	{
		try
		{
			string query = "SELECT cc.contract_id, cc.client_id, cc.loan_amount, \r\n                                cc.interest_rate, cc.loan_term_months, \r\n                                c.first_name, c.last_name\r\n                         FROM Credit_Contracts cc\r\n                         LEFT JOIN Clients c ON cc.client_id = c.client_id\r\n                         WHERE cc.status = 'Активен'  -- Только русское значение!\r\n                         ORDER BY cc.contract_id";
			contractsData = dbHelper.ExecuteQuery(query);
			if (contractsData.Rows.Count == 0)
			{
				query = "SELECT cc.contract_id, cc.client_id, cc.loan_amount, \r\n                             cc.interest_rate, cc.loan_term_months, \r\n                             c.first_name, c.last_name, cc.status\r\n                      FROM Credit_Contracts cc\r\n                      LEFT JOIN Clients c ON cc.client_id = c.client_id\r\n                      ORDER BY cc.contract_id";
				contractsData = dbHelper.ExecuteQuery(query);
				if (contractsData.Rows.Count == 0)
				{
					MessageBox.Show("В базе данных нет кредитных договоров!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки кредитных договоров: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetupContractsComboBox()
	{
		comboContractId.Items.Clear();
		if (contractsData == null || contractsData.Rows.Count == 0)
		{
			comboContractId.Items.Add("Нет кредитных договоров");
			comboContractId.Enabled = false;
			return;
		}
		comboContractId.Enabled = true;
		foreach (DataRow row in contractsData.Rows)
		{
			int contractId = Convert.ToInt32(row["contract_id"]);
			int clientId = Convert.ToInt32(row["client_id"]);
			decimal loanAmount = Convert.ToDecimal(row["loan_amount"]);
			string firstName = row["first_name"]?.ToString() ?? "Неизвестно";
			string lastName = row["last_name"]?.ToString() ?? "Неизвестно";
			string status = (row.Table.Columns.Contains("status") ? (row["status"]?.ToString() ?? "") : "Активен");
			string statusText = ((!string.IsNullOrEmpty(status)) ? (" [" + status + "]") : "");
			comboContractId.Items.Add($"Договор #{contractId}: {firstName} {lastName} - {loanAmount:C}{statusText}");
		}
		if (comboContractId.Items.Count > 0)
		{
			comboContractId.SelectedIndex = 0;
		}
	}

	private int GetSelectedContractId()
	{
		if (comboContractId.SelectedIndex < 0 || comboContractId.Items.Count == 0)
		{
			return -1;
		}
		string selectedText = comboContractId.SelectedItem.ToString();
		if (selectedText.Contains("Договор #"))
		{
			string[] parts = selectedText.Split(new string[1] { "Договор #" }, StringSplitOptions.None);
			if (parts.Length > 1)
			{
				string idPart = parts[1].Trim().Split(':')[0];
				if (int.TryParse(idPart, out var contractId))
				{
					return contractId;
				}
			}
		}
		return -1;
	}

	private void LoadPaymentTypes()
	{
		try
		{
			string query = "SELECT unnest(enum_range(NULL::payment_type)) as type_value";
			DataTable enumValues = dbHelper.ExecuteQuery(query);
			comboPaymentType.Items.Clear();
			foreach (DataRow row in enumValues.Rows)
			{
				comboPaymentType.Items.Add(row["type_value"].ToString());
			}
			if (comboPaymentType.Items.Count > 0)
			{
				comboPaymentType.SelectedIndex = 0;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Не удалось загрузить типы платежей: " + ex.Message + "\nИспользуются стандартные значения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			comboPaymentType.Items.Clear();
			ComboBox.ObjectCollection items = comboPaymentType.Items;
			object[] items2 = new string[5] { "По графику", "Досрочный", "Просроченный", "Частичный", "Полный" };
			items.AddRange(items2);
			comboPaymentType.SelectedIndex = 0;
		}
	}

	private void LoadData()
	{
		try
		{
			string query = "SELECT p.*, cc.client_id, \r\n                                        c.first_name || ' ' || c.last_name as client_name,\r\n                                        cc.loan_amount\r\n                                 FROM Payments p \r\n                                 LEFT JOIN Credit_Contracts cc ON p.contract_id = cc.contract_id\r\n                                 LEFT JOIN Clients c ON cc.client_id = c.client_id\r\n                                 ORDER BY p.payment_id";
			dataTable = dbHelper.ExecuteQuery(query);
			dataGridViewPayments.DataSource = dataTable;
			if (dataGridViewPayments.Columns.Contains("contract_id"))
			{
				dataGridViewPayments.Columns["contract_id"].HeaderText = "ID договора";
			}
			if (dataGridViewPayments.Columns.Contains("client_name"))
			{
				dataGridViewPayments.Columns["client_name"].HeaderText = "Клиент";
			}
			if (dataGridViewPayments.Columns.Contains("loan_amount"))
			{
				dataGridViewPayments.Columns["loan_amount"].HeaderText = "Сумма кредита";
			}
			if (dataGridViewPayments.Columns.Contains("payment_date"))
			{
				dataGridViewPayments.Columns["payment_date"].HeaderText = "Дата платежа";
			}
			if (dataGridViewPayments.Columns.Contains("actual_payment_date"))
			{
				dataGridViewPayments.Columns["actual_payment_date"].HeaderText = "Фактическая дата";
			}
			if (dataGridViewPayments.Columns.Contains("scheduled_amount"))
			{
				dataGridViewPayments.Columns["scheduled_amount"].HeaderText = "Сумма по графику";
			}
			if (dataGridViewPayments.Columns.Contains("paid_amount"))
			{
				dataGridViewPayments.Columns["paid_amount"].HeaderText = "Оплаченная сумма";
			}
			if (dataGridViewPayments.Columns.Contains("principal_amount"))
			{
				dataGridViewPayments.Columns["principal_amount"].HeaderText = "Основной долг";
			}
			if (dataGridViewPayments.Columns.Contains("interest_amount"))
			{
				dataGridViewPayments.Columns["interest_amount"].HeaderText = "Проценты";
			}
			if (dataGridViewPayments.Columns.Contains("payment_type"))
			{
				dataGridViewPayments.Columns["payment_type"].HeaderText = "Тип платежа";
			}
			if (dataGridViewPayments.Columns.Contains("payment_id"))
			{
				dataGridViewPayments.Columns["payment_id"].Visible = false;
			}
			if (dataGridViewPayments.Columns.Contains("client_id"))
			{
				dataGridViewPayments.Columns["client_id"].Visible = false;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		try
		{
			if (!ValidateRequiredFields())
			{
				return;
			}
			int contractId = GetSelectedContractId();
			if (contractId == -1)
			{
				MessageBox.Show("Выберите кредитный договор из списка!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			string paymentType = comboPaymentType.SelectedItem?.ToString() ?? "По графику";
			string query = "INSERT INTO Payments \r\n                        (contract_id, payment_date, scheduled_amount, \r\n                         paid_amount, principal_amount, interest_amount, payment_type) \r\n                         VALUES (@contractId, @paymentDate, @scheduledAmount, \r\n                         @paidAmount, @principalAmount, @interestAmount, @paymentType::payment_type)";
			NpgsqlParameter[] parameters = new NpgsqlParameter[7]
			{
				new NpgsqlParameter("@contractId", contractId),
				new NpgsqlParameter("@paymentDate", dtpPaymentDate.Value.Date),
				new NpgsqlParameter("@scheduledAmount", ParseDecimal(txtScheduledAmount.Text)),
				new NpgsqlParameter("@paidAmount", ParseDecimal(txtPaidAmount.Text)),
				new NpgsqlParameter("@principalAmount", ParseDecimal(txtPrincipalAmount.Text)),
				new NpgsqlParameter("@interestAmount", ParseDecimal(txtInterestAmount.Text)),
				new NpgsqlParameter("@paymentType", paymentType)
			};
			int result = dbHelper.ExecuteNonQuery(query, parameters);
			if (result > 0)
			{
				LoadData();
				ClearFields();
				MessageBox.Show("Платеж добавлен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			else
			{
				MessageBox.Show("Не удалось добавить платеж!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при добавлении платежа: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		if (dataGridViewPayments.CurrentRow != null)
		{
			try
			{
				if (ValidateRequiredFields())
				{
					int paymentId = Convert.ToInt32(dataGridViewPayments.CurrentRow.Cells["payment_id"].Value);
					int contractId = GetSelectedContractId();
					if (contractId == -1)
					{
						MessageBox.Show("Выберите кредитный договор из списка!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					else
					{
						string paymentType = comboPaymentType.SelectedItem?.ToString() ?? "По графику";
						string query = "UPDATE Payments SET \r\n                            contract_id=@contractId, payment_date=@paymentDate, \r\n                            scheduled_amount=@scheduledAmount, paid_amount=@paidAmount, \r\n                            principal_amount=@principalAmount, interest_amount=@interestAmount,\r\n                            payment_type=@paymentType::payment_type \r\n                            WHERE payment_id=@paymentId";
						NpgsqlParameter[] parameters = new NpgsqlParameter[8]
						{
							new NpgsqlParameter("@contractId", contractId),
							new NpgsqlParameter("@paymentDate", dtpPaymentDate.Value.Date),
							new NpgsqlParameter("@scheduledAmount", ParseDecimal(txtScheduledAmount.Text)),
							new NpgsqlParameter("@paidAmount", ParseDecimal(txtPaidAmount.Text)),
							new NpgsqlParameter("@principalAmount", ParseDecimal(txtPrincipalAmount.Text)),
							new NpgsqlParameter("@interestAmount", ParseDecimal(txtInterestAmount.Text)),
							new NpgsqlParameter("@paymentType", paymentType),
							new NpgsqlParameter("@paymentId", paymentId)
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
		MessageBox.Show("Выберите платеж для обновления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void btnDelete_Click(object sender, EventArgs e)
	{
		if (dataGridViewPayments.CurrentRow != null)
		{
			DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранный платеж?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				return;
			}
			try
			{
				int paymentId = Convert.ToInt32(dataGridViewPayments.CurrentRow.Cells["payment_id"].Value);
				string query = "DELETE FROM Payments WHERE payment_id=@paymentId";
				NpgsqlParameter[] parameters = new NpgsqlParameter[1]
				{
					new NpgsqlParameter("@paymentId", paymentId)
				};
				int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);
				if (rowsAffected > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Платеж удален успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось удалить платеж!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при удалении платежа: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите платеж для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void dataGridViewPayments_SelectionChanged(object sender, EventArgs e)
	{
		bool hasSelection = dataGridViewPayments.CurrentRow != null && dataGridViewPayments.CurrentRow.Index >= 0;
		btnUpdate.Enabled = hasSelection;
		btnDelete.Enabled = hasSelection;
		if (!hasSelection)
		{
			return;
		}
		DataGridViewRow row = dataGridViewPayments.CurrentRow;
		if (row.Cells["contract_id"].Value != null && row.Cells["contract_id"].Value != DBNull.Value)
		{
			int contractId = Convert.ToInt32(row.Cells["contract_id"].Value);
			bool found2 = false;
			for (int j = 0; j < comboContractId.Items.Count; j++)
			{
				if (GetContractIdFromItem(j) == contractId)
				{
					comboContractId.SelectedIndex = j;
					found2 = true;
					break;
				}
			}
			if (!found2 && comboContractId.Items.Count > 0)
			{
				comboContractId.SelectedIndex = 0;
			}
		}
		else if (comboContractId.Items.Count > 0)
		{
			comboContractId.SelectedIndex = 0;
		}
		if (row.Cells["payment_date"].Value != null && row.Cells["payment_date"].Value != DBNull.Value)
		{
			dtpPaymentDate.Value = Convert.ToDateTime(row.Cells["payment_date"].Value);
		}
		if (row.Cells["actual_payment_date"].Value != null && row.Cells["actual_payment_date"].Value != DBNull.Value)
		{
			dtpActualPaymentDate.Value = Convert.ToDateTime(row.Cells["actual_payment_date"].Value);
		}
		txtScheduledAmount.Text = FormatDecimal(row.Cells["scheduled_amount"].Value);
		txtPaidAmount.Text = FormatDecimal(row.Cells["paid_amount"].Value);
		txtPrincipalAmount.Text = FormatDecimal(row.Cells["principal_amount"].Value);
		txtInterestAmount.Text = FormatDecimal(row.Cells["interest_amount"].Value);
		if (row.Cells["payment_type"].Value != null && row.Cells["payment_type"].Value != DBNull.Value)
		{
			string paymentType = row.Cells["payment_type"].Value.ToString();
			bool found = false;
			for (int i = 0; i < comboPaymentType.Items.Count; i++)
			{
				if (comboPaymentType.Items[i].ToString() == paymentType)
				{
					comboPaymentType.SelectedIndex = i;
					found = true;
					break;
				}
			}
			if (!found && comboPaymentType.Items.Count > 0)
			{
				comboPaymentType.SelectedIndex = 0;
			}
		}
		else if (comboPaymentType.Items.Count > 0)
		{
			comboPaymentType.SelectedIndex = 0;
		}
	}

	private int GetContractIdFromItem(int index)
	{
		if (index < 0 || index >= comboContractId.Items.Count)
		{
			return -1;
		}
		string itemText = comboContractId.Items[index].ToString();
		if (itemText.Contains("Договор #"))
		{
			string[] parts = itemText.Split(new string[1] { "Договор #" }, StringSplitOptions.None);
			if (parts.Length > 1)
			{
				string idPart = parts[1].Trim().Split(':')[0];
				if (int.TryParse(idPart, out var contractId))
				{
					return contractId;
				}
			}
		}
		return -1;
	}

	private void ClearFields()
	{
		if (comboContractId.Items.Count > 0)
		{
			comboContractId.SelectedIndex = 0;
		}
		dtpPaymentDate.Value = DateTime.Now;
		dtpActualPaymentDate.Value = DateTime.Now;
		txtScheduledAmount.Clear();
		txtPaidAmount.Clear();
		txtPrincipalAmount.Clear();
		txtInterestAmount.Clear();
		if (comboPaymentType.Items.Count > 0)
		{
			comboPaymentType.SelectedIndex = 0;
		}
		dataGridViewPayments.ClearSelection();
	}

	private bool ValidateRequiredFields()
	{
		if (comboContractId.SelectedIndex < 0 || comboContractId.Items.Count == 0)
		{
			MessageBox.Show("Выберите кредитный договор из списка!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			comboContractId.Focus();
			return false;
		}
		if (!ValidateDecimal(txtScheduledAmount.Text, "Сумма по графику"))
		{
			return false;
		}
		if (!ValidateDecimal(txtPaidAmount.Text, "Оплаченная сумма", allowZero: true))
		{
			return false;
		}
		if (!ValidateDecimal(txtPrincipalAmount.Text, "Основной долг"))
		{
			return false;
		}
		if (!ValidateDecimal(txtInterestAmount.Text, "Проценты"))
		{
			return false;
		}
		if (comboPaymentType.SelectedItem == null)
		{
			MessageBox.Show("Выберите тип платежа!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			comboPaymentType.Focus();
			return false;
		}
		return true;
	}

	private bool ValidateDecimal(string value, string fieldName, bool allowZero = false)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			MessageBox.Show(fieldName + " должна быть указана!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return false;
		}
		if (!decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			MessageBox.Show(fieldName + " должна быть числом!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return false;
		}
		if (allowZero)
		{
			if (result < 0m)
			{
				MessageBox.Show(fieldName + " не может быть отрицательной!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}
		else if (result <= 0m)
		{
			MessageBox.Show(fieldName + " должна быть положительным числом!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return false;
		}
		return true;
	}

	private decimal ParseDecimal(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return 0m;
		}
		return decimal.Parse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
	}

	private string FormatDecimal(object value)
	{
		if (value == null || value == DBNull.Value)
		{
			return "";
		}
		return Convert.ToDecimal(value).ToString("0.00", CultureInfo.InvariantCulture);
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		ClearFields();
	}

	private void btnRefreshContracts_Click(object sender, EventArgs e)
	{
		try
		{
			LoadContracts();
			SetupContractsComboBox();
			MessageBox.Show("Список кредитных договоров обновлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при обновлении списка договоров: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
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
		this.dataGridViewPayments = new System.Windows.Forms.DataGridView();
		this.dtpPaymentDate = new System.Windows.Forms.DateTimePicker();
		this.dtpActualPaymentDate = new System.Windows.Forms.DateTimePicker();
		this.txtScheduledAmount = new System.Windows.Forms.TextBox();
		this.txtPaidAmount = new System.Windows.Forms.TextBox();
		this.txtPrincipalAmount = new System.Windows.Forms.TextBox();
		this.txtInterestAmount = new System.Windows.Forms.TextBox();
		this.comboPaymentType = new System.Windows.Forms.ComboBox();
		this.btnRefreshContracts = new System.Windows.Forms.Button();
		this.btnUpdate = new System.Windows.Forms.Button();
		this.btnDelete = new System.Windows.Forms.Button();
		this.comboContractId = new System.Windows.Forms.ComboBox();
		this.btnRefreshData = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPayments).BeginInit();
		base.SuspendLayout();
		this.dataGridViewPayments.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewPayments.Location = new System.Drawing.Point(0, 80);
		this.dataGridViewPayments.Name = "dataGridViewPayments";
		this.dataGridViewPayments.Size = new System.Drawing.Size(1276, 385);
		this.dataGridViewPayments.TabIndex = 0;
		this.dtpPaymentDate.Location = new System.Drawing.Point(127, 41);
		this.dtpPaymentDate.Name = "dtpPaymentDate";
		this.dtpPaymentDate.Size = new System.Drawing.Size(200, 23);
		this.dtpPaymentDate.TabIndex = 2;
		this.dtpActualPaymentDate.Location = new System.Drawing.Point(333, 41);
		this.dtpActualPaymentDate.Name = "dtpActualPaymentDate";
		this.dtpActualPaymentDate.Size = new System.Drawing.Size(200, 23);
		this.dtpActualPaymentDate.TabIndex = 3;
		this.txtScheduledAmount.Location = new System.Drawing.Point(539, 41);
		this.txtScheduledAmount.Name = "txtScheduledAmount";
		this.txtScheduledAmount.Size = new System.Drawing.Size(111, 23);
		this.txtScheduledAmount.TabIndex = 4;
		this.txtPaidAmount.Location = new System.Drawing.Point(656, 40);
		this.txtPaidAmount.Name = "txtPaidAmount";
		this.txtPaidAmount.Size = new System.Drawing.Size(100, 23);
		this.txtPaidAmount.TabIndex = 4;
		this.txtPrincipalAmount.Location = new System.Drawing.Point(762, 40);
		this.txtPrincipalAmount.Name = "txtPrincipalAmount";
		this.txtPrincipalAmount.Size = new System.Drawing.Size(100, 23);
		this.txtPrincipalAmount.TabIndex = 4;
		this.txtInterestAmount.Location = new System.Drawing.Point(868, 40);
		this.txtInterestAmount.Name = "txtInterestAmount";
		this.txtInterestAmount.Size = new System.Drawing.Size(100, 23);
		this.txtInterestAmount.TabIndex = 4;
		this.comboPaymentType.FormattingEnabled = true;
		this.comboPaymentType.Location = new System.Drawing.Point(974, 40);
		this.comboPaymentType.Name = "comboPaymentType";
		this.comboPaymentType.Size = new System.Drawing.Size(121, 23);
		this.comboPaymentType.TabIndex = 5;
		this.btnRefreshContracts.Location = new System.Drawing.Point(1191, 39);
		this.btnRefreshContracts.Name = "btnRefreshContracts";
		this.btnRefreshContracts.Size = new System.Drawing.Size(75, 23);
		this.btnRefreshContracts.TabIndex = 6;
		this.btnRefreshContracts.Text = "Добавить";
		this.btnRefreshContracts.UseVisualStyleBackColor = true;
		this.btnUpdate.Location = new System.Drawing.Point(1110, 487);
		this.btnUpdate.Name = "btnUpdate";
		this.btnUpdate.Size = new System.Drawing.Size(75, 46);
		this.btnUpdate.TabIndex = 7;
		this.btnUpdate.Text = "изменить";
		this.btnUpdate.UseVisualStyleBackColor = true;
		this.btnDelete.Location = new System.Drawing.Point(1191, 487);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(75, 46);
		this.btnDelete.TabIndex = 8;
		this.btnDelete.Text = "Удалить";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.comboContractId.FormattingEnabled = true;
		this.comboContractId.Location = new System.Drawing.Point(0, 40);
		this.comboContractId.Name = "comboContractId";
		this.comboContractId.Size = new System.Drawing.Size(121, 23);
		this.comboContractId.TabIndex = 9;
		this.btnRefreshData.Location = new System.Drawing.Point(1110, 40);
		this.btnRefreshData.Name = "btnRefreshData";
		this.btnRefreshData.Size = new System.Drawing.Size(75, 23);
		this.btnRefreshData.TabIndex = 10;
		this.btnRefreshData.Text = "Обновить";
		this.btnRefreshData.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 22);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(72, 15);
		this.label1.TabIndex = 11;
		this.label1.Text = "ID договора";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(173, 22);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(79, 15);
		this.label2.TabIndex = 11;
		this.label2.Text = "дата платежа";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(380, 22);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(73, 15);
		this.label3.TabIndex = 11;
		this.label3.Text = "Дата сейчас";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(539, 22);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(111, 15);
		this.label4.TabIndex = 11;
		this.label4.Text = "Сумма по графику";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(684, 22);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(47, 15);
		this.label5.TabIndex = 11;
		this.label5.Text = "Оплата";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(762, 22);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(91, 15);
		this.label6.TabIndex = 11;
		this.label6.Text = "Основной долг";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(889, 22);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(55, 15);
		this.label7.TabIndex = 11;
		this.label7.Text = "Процент";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(889, 22);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(55, 15);
		this.label8.TabIndex = 11;
		this.label8.Text = "Процент";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(997, 22);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(77, 15);
		this.label9.TabIndex = 11;
		this.label9.Text = "Тип платежа";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1279, 545);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.btnRefreshData);
		base.Controls.Add(this.comboContractId);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnUpdate);
		base.Controls.Add(this.btnRefreshContracts);
		base.Controls.Add(this.comboPaymentType);
		base.Controls.Add(this.txtInterestAmount);
		base.Controls.Add(this.txtPrincipalAmount);
		base.Controls.Add(this.txtPaidAmount);
		base.Controls.Add(this.txtScheduledAmount);
		base.Controls.Add(this.dtpActualPaymentDate);
		base.Controls.Add(this.dtpPaymentDate);
		base.Controls.Add(this.dataGridViewPayments);
		base.Name = "PaymentsForm";
		this.Text = "Payments";
		((System.ComponentModel.ISupportInitialize)this.dataGridViewPayments).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

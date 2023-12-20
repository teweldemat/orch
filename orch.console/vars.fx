{
  "sys":{ "SystemId":"c0662424-ad91-439d-a260-ec22e39a51a9", "RootPasword":"pass"},
  "vouchers_from":1,
  "vouchers_to":99999,
  "vouchers":[
    [ "BEG", "BEGINNING BALANCE JOURNAL" ],
    [ "PV", "PAYMENT VOUCHER" ],
    [ "CPV", "CHECK PAYMENT VOUCHER" ],
    [ "PCP", "PETTY CASH PAYMENT VOUCHER" ],
    [ "BTV", "BANK TRANSFER VOUCHER" ],
    [ "GJV", "GENERAL JOURNAL VOUCHER" ],
    [ "CRV", "CASH RECEIPT VOUCHER" ],
    [ "CRS", "CREDIT SALES VOUCHER" ],
    [ "BDS", "BANK DEPOSIT SLIP VOUCHER" ],
    [ "TPV", "TEMPORARY PAYMENT VOUCHER" ],
    [ "MBR", "MONTHLY BILL RECEIVING VOUCHER" ],
    [ "BIV", "BILL ISSUE VOUCHER" ],
    [ "BRR", "BILL RETURN AND RECEIVING VOUCHER" ],
    [ "PR", "PURCHASE REQUISITION VOUCHER" ],
    [ "POV", "PURCHASE ORDER VOUCHER" ],
    [ "GRV", "GOODS RECEIVING VOUCHER" ],
    [ "SRV", "STORE REQUISITION VOUCHER" ],
    [ "SIV", "STORE ISSUE VOUCHER" ],
    [ "STV", "STORE TRANSFER VOUCHER" ],
    [ "STRV", "STORE TRANSFER RECEIVING VOUCHER" ],
    [ "SRT", "STORE RETURN VOUCHER" ],
    [ "GDN", "GOODS DELIVERY NOTE JOURNAL VOUCHER" ],
    [ "FUL", "FUEL AND LUBRICANTS VOUCHER" ],
    [ "EXP", "EXPENSE ADVANCE VOUCHER" ]
  ],
  "company":{ "Name":"Unnamed Company"},
  "main_accounts":[
    [ "1000", "Asset", "AST", true ],
    [ "2000", "Liability", "LB", false ],
    [ "3000", "Capital", "CAP", false ],
    [ "4000", "Revenue", "RV", false ],
    [ "5000", "Expense", "EXP", true ],
    [ "6000", "Cost", "CST", true ]
  ],
  "finops_permissions":[
    [ "FIN_PR_ADMIN", "Payment Process Administration" ],
    [ "FIN_REQUEST_PAYMENT", "Payment Request" ],
    [ "FIN_PR_ANALYASIS", "Payment Request Analysis" ],
    [ "FIN_PR_APPROVE_BUDGET", "Budget Request Approval" ],
    [ "FIN_PR_APPROVE_PAYMENT", "Payment Approval" ],
    [ "FIN_PR_PAY", "Payment" ],
    [ "FIN_PR_BOOK_KEEPING", "Payment Bookkeeing" ],
    [ "FIN_PR_POST_PAYMENT_LEDGER_ENTRIES", "Post Post-Payment Ledger Entries" ],
    [ "FIN_PR_CLOSE", "Close Payment Request" ],
    [ "FIN_PR_CANCEL", "Cancel Payment Request" ],
    [ "FIN_PAYMENT_BOOK", "Book Payment" ],
    [ "FIN_REQUEST_EXPENSE_ADVANCE", "Request Expense Advance" ],
    [ "FIN_EXPENSE_ADVANCE_ADMIN", "Administer Expense Advace Process" ],
    [ "FIN_APPROVE_EXPENSE_ADVANCE", "Approve Expense Advace" ],
    [ "FIN_ANALYZE_EXPENSE_ADVANCE_REPORT", "Analyze Expense Advance Report" ],
    [ "FIN_PURCHASE_ADMIN", "Administer Purchase Administration" ],
    [ "FIN_RECORD_PURCHASE", "Record purchase" ],
    [ "FIN_REGISTER_PER_DIEM_EXPENSE", "Record Per Diem Expense" ]
  ],
  "hr_permissions":[
    [ "HR_ADMIN", "HR Administartor" ],
    [ "HRM_EMPLOYEE_PROFILE", "Manage Employee Profile" ],
    [ "HRM_EMPLOYEE_ATTENDANCE", "Manage Employee Attendance" ],
    [ "HRM_EMPLOYEE_PERFORMANCE", "Manage Employee Profile" ],
    [ "HRM_EMPLOYEE_BENEFIT", "Manage Employee Benefits" ],
    [ "HRM_ORGANIZATION", "Organizational Stracture" ],
    [ "HRM_CLOSE_VACANCY", "Close Vacancey" ],
    [ "HRM_VACANCY", "Vacancey" ],
    [ "HRM_OPEN_VACANCY", "Open Vacancey" ],
    [ "HRM_APPLICANT", "Create Application" ],
    [ "HRM_HIRE_APPLICANT", "Hire Applicant" ],
    [ "HR_EMPLOYEE_DISCIPLINE", "Discipline Management" ],
    [ "HRM_LEAVE_BALANCE", "Leave Balance" ],
    [ "HR_REQUEST_LEAVE", "Leave Request" ],
    [ "HR_LR_CHECK", "Leave Request Status" ],
    [ "HR_LR_MODIFY", "Modify Leave Request" ],
    [ "HR_LR_APPROVE", "Approve Leave Request" ],
    [ "HR_LR_CLOSE", "Close Leave Request" ],
    [ "HR_LR_CANCEL", "Cancel Leave Request" ],
    [ "HR_LR_ADMIN", "Leave Request Management" ],
    [ "LEAVE_PROCCESS_CONFIG", "Configure Leave Process" ],
    [ "LR", "Leave Request Serila No" ],
    [ "HRM_LEAVE_PLAN", "Leave Plan" ]
  ],
  "org":{
    "Name":"Webketema Water Utility",
    "LocalName":"ውብከተማ ዉሃና ፍሳሽ አገልግሎት",
    "Code":"WU",
    "Abbreviation":"WWU",
    "Email":"info@wwu.org",
    "Remark":"This is the water utility for the town of Webketema",
    "Moto":"Water for Life",
    "PhoneNumber":"+251-12345678",
    "Address":"Webketema, Ethiopia",
    "Mission":"To provide clean, reliable and affordable water to our customers",
    "Values":"Integrity, professionalism, customer focus, innovation",
    "Vision":"To be the best water utility in the region",
    "TinNumber":"1234567890"
  },
  "budget_items":[
    [ "4001", "Office Expense", true ],
    [ "4002", "Telecom Bills", true ],
    [ "4003", "Financial Operation", true ],
    [ "4004", "Mission Expense", true ]
  ],
  "accounts":[
    [ "1010", "Cash", true, "1000" ],
    [ "1020", "Bank", true, "1000" ],
    [ "1030", "Staff Short Term loan", false, "1000" ],
    [ "1040", "Staff Long Term loan", false, "1000" ],
    [ "1050", "Supplier Receivable", false, "1000" ],
    [ "1160", "Goods In Transit", false, "1000" ],
    [ "1110", "Inventory", false, "1000" ],
    [ "1120", "Fixed Assets", false, "1000" ],
    [ "2010", "Payroll Deductions", false, "2000" ],
    [ "2020", "Withheld Tax", false, "2000" ],
    [ "2030", "Supplier Payable", false, "2000" ],
    [ "2040", "Supplier Retention", false, "2000" ],
    [ "2050", "Supplier Bonds", false, "2000" ],
    [ "4140", "Customer Service Revenue Items", false, "4000" ],
    [ "5010", "Salary Expense", false, "5000" ],
    [ "5020", "Perdiem Expense", false, "5000" ],
    [ "5110", "Issued Inventory", false, "5000" ],
    [ "5120", "Fixed Asset Depriciation", false, "5000" ],
    [ "5130", "Itemized Expenses", false, "5000" ],
    [ "6010", "Salary Expense", false, "6000" ],
    [ "6020", "Perdiem Expense", false, "6000" ],
    [ "6110", "Issued Inventory", false, "6000" ],
    [ "6120", "Fixed Asset Depriciation", false, "6000" ],
    [ "6130", "Itemized Expenses", false, "6000" ]
  ],
  "sl_accounts":[
    [ "1011", "Petty Cash", true, "1010", null ],
    [ "1012", "Main Cash", true, "1010", null ],
    [ "1021", "CBE Account", true, "1020", null ],
    [ "1022", "Abyssinia Bank Account", true, "1020", null ],
    [ "1111", "Pipe and Fittings", false, "1110", "4001" ],
    [ "1112", "Stationary", false, "1110", "4002" ],
    [ "1121", "Vehicles", false, "1120", "4001" ],
    [ "1122", "Machinary", false, "1120", "4001" ],
    [ "2021", "VAT", false, "2020", null ],
    [ "2022", "WHT", false, "2020", null ],
    [ "4141", "Technical Service Charge", false, "4140", null ],
    [ "4142", "Other Service Charge", false, "4140", null ],
    [ "5111", "Issued Pipe and Fittings", false, "5110", null ],
    [ "5112", "Issued Stationary", false, "5110", null ],
    [ "5121", "Vehicles Depriciation", false, "5120", null ],
    [ "5122", "Machinary Depriciation", false, "5120", null ],
    [ "5131", "Hotel Accomodation", false, "5130", "4004" ],
    [ "5132", "Office Expenses", false, "5130", "4002" ],
    [ "5133", "Financial Service Expenses", false, "5130", "4003" ],
    [ "6111", "Cost Issued Pipe and Fittings", false, "6110", null ],
    [ "6112", "Issued Stationary", false, "6110", null ],
    [ "6121", "Cost Vehicles Depriciation", false, "6120", null ],
    [ "6122", "Cost Machinary Depriciation", false, "6120", null ],
    [ "6131", "Production Plant Cost", false, "6130", null ],
    [ "6132", "Production Labor Cost", false, "6130", null ]
  ],
  "main_categories":[
    { "Code":"10", "Name":"Expense Items"},
    { "Code":"20", "Name":"Inventory"},
    { "Code":"30", "Name":"Fixed Asset"},
    { "Code":"40", "Name":"Sales Items"}
  ],
  "sub_categories":[
    {
      "code":"11",
      "name":"Hotel Accomodation Category",
      "parent":"10",
      "accounts":[ "5131", null, null, null, null ]
    },
    {
      "code":"12",
      "name":"Office Expenses Category",
      "parent":"10",
      "accounts":[ "5132", null, null, null, null ]
    },
    {
      "code":"13",
      "name":"Financial Service Expense Category",
      "parent":"10",
      "accounts":[ "5133", null, null, null, null ]
    },
    {
      "code":"21",
      "name":"Pipe and Fittings Inventory",
      "parent":"20",
      "accounts":[ "5111", "1111", null, null, null ]
    },
    {
      "code":"22",
      "name":"Stationary Inventory",
      "parent":"20",
      "accounts":[ "5112", "1112", null, null, null ]
    },
    {
      "code":"31",
      "name":"Vehicles Fixed Asset",
      "parent":"30",
      "accounts":[ null, null, "1121", "5121", null ]
    },
    {
      "code":"32",
      "name":"Machinary Fixed Asset",
      "parent":"30",
      "accounts":[ null, null, "1122", "5122", null ]
    },
    {
      "code":"41",
      "name":"Technical Service Charge",
      "parent":"40",
      "accounts":[ null, null, null, null, "4141" ]
    },
    {
      "code":"42",
      "name":"Other Service Charge",
      "parent":"40",
      "accounts":[ null, null, null, null, "4142" ]
    }
  ],
  "line_items":[
    {
      "code":"11-1",
      "name":"Hotel Accomodation Category",
      "category":"11",
      "IsMaterial":false,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"11-2",
      "name":"Tax Free Hotel Accomodation Category",
      "category":"11",
      "IsMaterial":false,
      "IsFixedAsset":false,
      "TaxCategory":"NoneTaxable"
    },
    {
      "code":"12-1",
      "name":"Office Expenses Category",
      "category":"12",
      "IsMaterial":false,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"13-1",
      "name":"Financial Service Expense Category",
      "category":"13",
      "IsMaterial":false,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"21-1",
      "name":"Pipe",
      "category":"21",
      "IsMaterial":true,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"21-2",
      "name":"Fitting",
      "category":"21",
      "IsMaterial":true,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"21-3",
      "name":"Other",
      "category":"21",
      "IsMaterial":true,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"22-1",
      "name":"Paper",
      "category":"22",
      "IsMaterial":true,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"22-2",
      "name":"Printer Ink",
      "category":"22",
      "IsMaterial":true,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"22-3",
      "name":"Other",
      "category":"22",
      "IsMaterial":true,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"31-1",
      "name":"Automobile",
      "category":"31",
      "IsMaterial":true,
      "IsFixedAsset":true,
      "TaxCategory":"Taxable"
    },
    {
      "code":"31-2",
      "name":"Other Vehicle",
      "category":"31",
      "IsMaterial":true,
      "IsFixedAsset":true,
      "TaxCategory":"Taxable"
    },
    {
      "code":"32-1",
      "name":"Generator",
      "category":"32",
      "IsMaterial":true,
      "IsFixedAsset":true,
      "TaxCategory":"Taxable"
    },
    {
      "code":"32-2",
      "name":"Pump",
      "category":"32",
      "IsMaterial":true,
      "IsFixedAsset":true,
      "TaxCategory":"Taxable"
    },
    {
      "code":"41-1",
      "name":"Excavation",
      "category":"41",
      "IsMaterial":false,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"41-2",
      "name":"Estimation",
      "category":"41",
      "IsMaterial":false,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    },
    {
      "code":"42-1",
      "name":"Other Technical",
      "category":"42",
      "IsMaterial":false,
      "IsFixedAsset":false,
      "TaxCategory":"Taxable"
    }
  ],
  "pay_config":{
    "ActionFormulas":[
      {
        "ActionTypeId":"a990646f-ca0d-482f-87b4-ee1c30a15845",
        "Formula":{
          "ActionCheckFormula":"wfstate=null or wfstate.Status in ['Submitted','ModificationRequested']",
          "UserCheckFormula":"wfstate=null or Any(wfstate.PerformedActions,(x)=>x.ActionType='SubmitRequest' and x.UserId=user.Id)",
          "UserActionCheckFormula":"wfstate=null or Any(wfstate.PerformedActions,(x)=>x.ActionType='SubmitRequest' and x.UserId=user.Id)"
        }
      },
      {
        "ActionTypeId":"6639e399-1dff-4828-b131-27c5a85bdfe5",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Submitted','AnalysisStarted'] ",
          "UserCheckFormula":"wfstate.Status='Submitted' or (wfstate.Status='AnalysisStarted' and ({ firstaction:First(finops_payment.GetPrHistoryWithData(wfstate.taskid),(x)=>x.MainCommand and (x.datatype in ['FIN_PR_SET_BUDGET','FIN_PR_SET_LEDGER_ENTRIES','FIN_PR_ASSIGN_SOURCE','FIN_PR_ASSIGN_SOURCE','FIN_PR_FINISH_ANALYSIS'])), return if(firstaction=null,true,firstaction.UserId=user.Id)}))",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"76240556-05d5-4a83-9f68-e9e74a6c631b",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Submitted','AnalysisStarted'] ",
          "UserCheckFormula":"wfstate.Status='Submitted' or (wfstate.Status='AnalysisStarted' and ({ firstaction:First(finops_payment.GetPrHistoryWithData(wfstate.taskid),(x)=>x.MainCommand and (x.datatype in ['FIN_PR_SET_BUDGET','FIN_PR_SET_LEDGER_ENTRIES','FIN_PR_ASSIGN_SOURCE','FIN_PR_ASSIGN_SOURCE','FIN_PR_FINISH_ANALYSIS'])), return if(firstaction=null,true,firstaction.UserId=user.Id)}))",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"5e564aee-fef3-4d20-9a1c-2d70f6858b2f",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Submitted','AnalysisStarted'] ",
          "UserCheckFormula":"wfstate.Status='Submitted' or (wfstate.Status='AnalysisStarted' and ({ firstaction:First(finops_payment.GetPrHistoryWithData(wfstate.taskid),(x)=>x.MainCommand and (x.datatype in ['FIN_PR_SET_BUDGET','FIN_PR_SET_LEDGER_ENTRIES','FIN_PR_ASSIGN_SOURCE','FIN_PR_ASSIGN_SOURCE','FIN_PR_FINISH_ANALYSIS'])), return if(firstaction=null,true,firstaction.UserId=user.Id)}))",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"7fe88b19-2e8c-42c5-be6d-8959997f5d95",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Submitted','AnalysisStarted'] ",
          "UserCheckFormula":"wfstate.Status='Submitted' or (wfstate.Status='AnalysisStarted' and ({ firstaction:First(finops_payment.GetPrHistoryWithData(wfstate.taskid),(x)=>x.MainCommand and (x.datatype in ['FIN_PR_SET_BUDGET','FIN_PR_SET_LEDGER_ENTRIES','FIN_PR_ASSIGN_SOURCE','FIN_PR_ASSIGN_SOURCE','FIN_PR_FINISH_ANALYSIS'])), return if(firstaction=null,true,firstaction.UserId=user.Id)}))",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"505c7eec-cec5-4035-898b-648a8f3f1c72",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['AnalysisStarted'] ",
          "UserCheckFormula":"wfstate.Status='Submitted' or (wfstate.Status='AnalysisStarted' and ({ firstaction:First(finops_payment.GetPrHistoryWithData(wfstate.taskid),(x)=>x.MainCommand and (x.datatype in ['FIN_PR_SET_BUDGET','FIN_PR_SET_LEDGER_ENTRIES','FIN_PR_ASSIGN_SOURCE','FIN_PR_ASSIGN_SOURCE','FIN_PR_FINISH_ANALYSIS'])), return if(firstaction=null,true,firstaction.UserId=user.Id)}))",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"699b8e36-ae98-42b2-827d-ec89c4048eb7",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Analyzed']",
          "UserCheckFormula":"true",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"f63e0468-8d15-413e-852a-484e268c84df",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Analyzed']",
          "UserCheckFormula":"true",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"f7f818c9-b5b7-40d5-99d8-7f2f58519fff",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['BudgetApproved'] ",
          "UserCheckFormula":"true",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"1938232c-b896-46dd-8b52-b9f34ee58680",
        "Formula":{
          "ActionCheckFormula":"false",
          "UserCheckFormula":"false",
          "UserActionCheckFormula":"false"
        }
      },
      {
        "ActionTypeId":"6fd57ee8-9a6b-4ee3-b2af-eb6a13f3c37a",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Paid'] ",
          "UserCheckFormula":"true",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"dc38d9e0-a3ea-4285-8bc3-9354348b035c",
        "Formula":{
          "ActionCheckFormula":"wfstate=null or wfstate.Status in ['ApprovedForPayment']",
          "UserCheckFormula":"true",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"a1cdceb4-9be6-4681-ad1a-6d46fd378541",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Booked'] ",
          "UserCheckFormula":"true",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"a7c7dbc0-67e8-11ed-979c-f889d284fa14",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['BudgetApproved'] ",
          "UserCheckFormula":"true",
          "UserActionCheckFormula":"true"
        }
      },
      {
        "ActionTypeId":"026d3e90-f1c3-48a8-8c1b-84c9e873b15c",
        "Formula":{
          "ActionCheckFormula":"false",
          "UserCheckFormula":"false",
          "UserActionCheckFormula":"false"
        }
      },
      {
        "ActionTypeId":"1fe8639d-d359-4944-838a-41b48f6d12ca",
        "Formula":{
          "ActionCheckFormula":"wfstate!=null and wfstate.Status in ['Submitted','AnalysisStarted'] ",
          "UserCheckFormula":"wfstate.Status='Submitted' or (wfstate.Status='AnalysisStarted' and ({ firstaction:First(finops_payment.GetPrHistoryWithData(wfstate.taskid),(x)=>x.MainCommand and (x.datatype in ['FIN_PR_SET_BUDGET','FIN_PR_SET_LEDGER_ENTRIES','FIN_PR_ASSIGN_SOURCE','FIN_PR_ASSIGN_SOURCE','FIN_PR_FINISH_ANALYSIS'])), return if(firstaction=null,true,firstaction.UserId=user.Id)}))",
          "UserActionCheckFormula":"true"
        }
      }
    ]
  },
  "wh_types":[
    { "Key":"VAT", "Name":"Withheld VAT", "Account":"2021"},
    { "Key":"WHT", "Name":"Withheld Tax", "Account":"2022"}
  ],
  "cash_stores":[ [ "1012", "CashOnHand" ], [ "1021", "BankAccount" ] ],
  "supplier_categories":[
    { "Key":"CAT-1", "Name":"Tax Registered Suppliers"},
    { "Key":"CAT-2", "Name":"Unregistered Suppliers"}
  ],
  "suppliers":[
    {
      "Code":"SUP-1",
      "Name":"Tax Registered Supplier 1",
      "TaxType":"TOTRegistered",
      "TIN":"12345678",
      "PhoneNo":"+251-911111111",
      "Email":"info@supplier1.com"
    },
    {
      "Code":"SUP-2",
      "Name":"Tax Registered Supplier 2",
      "TaxType":"VATRegistered",
      "TIN":"12345679",
      "VATNumber":"ET123456",
      "PhoneNo":"+251-911111111",
      "Email":"info@supplier1.com"
    },
    {
      "Code":"SUP-3",
      "Name":"Unregistered Supplier 1",
      "TaxType":"NoTinTaxpayer",
      "PhoneNo":"+251-911111111",
      "Email":"info@supplier1.com"
    },
    {
      "Code":"SUP-4",
      "Name":"Unregistered Supplier 2",
      "TaxType":"NoneTaxPayer",
      "PhoneNo":"+251-911111111",
      "Email":"info@supplier1.com"
    }
  ],
  "lookups":[
    { "Code":"1", "Description":"Pcs"},
    { "Code":"2", "Description":"Gm"},
    { "Code":"3", "Description":"KG"},
    { "Code":"4", "Description":"Meter"},
    { "Code":"5", "Description":"Number"},
    { "Code":"6", "Description":"kuntal"},
    { "Code":"7", "Description":"LITTER"},
    { "Code":"8", "Description":"packet"},
    { "Code":"9", "Description":"set"},
    { "Code":"10", "Description":"Bottle"},
    { "Code":"11", "Description":"Kit"},
    { "Code":"12", "Description":"paire"},
    { "Code":"13", "Description":"Care"},
    { "Code":"14", "Description":"Pair"},
    { "Code":"15", "Description":"roll"},
    { "Code":"16", "Description":"Amper"},
    { "Code":"17", "Description":"Gallon"},
    { "Code":"18", "Description":"pad"},
    { "Code":"19", "Description":"Tikili"},
    { "Code":"20", "Description":"Desta"},
    { "Code":"21", "Description":"Derzen"},
    { "Code":"22", "Description":"Paket"},
    { "Code":"23", "Description":"Barega"}
  ],
  "core_organization":{
    "Id":"200a91c2-ef25-442a-9130-886283f153a9",
    "Code":"WU",
    "Name":"Webketema Water Utility",
    "LocalName":"ውብከተማ ዉሃና ፍሳሽ አገልግሎት",
    "Abbreviation":"WWU",
    "Vision":"To be the best water utility in the region",
    "Mission":"To provide clean, reliable and affordable water to our customers",
    "Values":"Integrity, professionalism, customer focus, innovation",
    "LogoFileId":"e2a06cff-2ec4-430e-b098-e00e09132c26",
    "LeftLetterHeadingLogoFileId":null,
    "RightLetterHeadingLogoFileId":null,
    "Moto":"Water for Life",
    "Address":"Webketema, Ethiopia",
    "TinNumber":"1234567890",
    "PhoneNumber":"+251-12345678",
    "Email":"info@wwu.org",
    "VatRegistrationNumber":null,
    "WorkingHrPerMonth":"0",
    "PerdiumRate":"Fixed",
    "Remark":"This is the water utility for the town of Webketema",
    "CreateTime":"20230420114015625",
    "CreatCommandId":"67fbd84e-3a15-4bf5-b75c-40d04f16fab4",
    "UpdateTime":"20230420114015625",
    "UpdateCommandId":"67fbd84e-3a15-4bf5-b75c-40d04f16fab4"
  },
  "core_organization_structure":{
    "Id":"a9deb298-6a43-4587-a95d-3612e3195d9b",
    "StructureName":"Webketema Water Utility",
    "LocalName":"ውብከተማ ዉሃና ፍሳሽ አገልግሎት",
    "OrganizationalStructureId":"a9deb298-6a43-4587-a95d-3612e3195d9b",
    "OrganizationId":"200a91c2-ef25-442a-9130-886283f153a9",
    "Remark":null,
    "Code":null,
    "CreateTime":"0",
    "CreatCommandId":"00000000-0000-0000-0000-000000000000",
    "UpdateTime":"0",
    "UpdateCommandId":"00000000-0000-0000-0000-000000000000"
  },
  "hr_divisions":[
    { "DivID":1, "Division":"ዋና ሥራ አስኪያጅ"},
    { "DivID":2, "Division":"የመጠጥ ውሃ አቅርቦት ተቋማት አስ/የስራ ሂደት"},
    { "DivID":3, "Division":"የገቢ፤ግዥ፤የፋይናንስ፤ንብረትና አስተዳደር የስራ ሂደት"},
    { "DivID":4, "Division":"ቅርንጫፍ ጽ/ቤት"}
  ],
  "hr_departments":[
    { "DepID":11, "DivID":1, "Department":"የስራ አስኪያጅ ጽ/ቤት"},
    { "DepID":12, "DivID":1, "Department":"የህግ አገልግሎት እና የስነ-ምግባር ኬዝ ቲም"},
    { "DepID":13, "DivID":1, "Department":"የውስጥ ኦዲት አገልግሎት ኬዝ ቲም"},
    { "DepID":21, "DivID":2, "Department":"የመጠጥ ውሃ አቅርቦትና የተቋማት አስ/የስራ ሂደት"},
    { "DepID":22, "DivID":2, "Department":"የፍሳሽ ቆሻሻ አወጋገድ እና አስተዳደር ዋና የስራ ሂደት"},
    {
      "DepID":23,
      "DivID":2,
      "Department":"የውሃ ምርት፤ጥራት፤ ስርጭት እና ብክነት ቁጥጥር ንዑስ የስራ ሂደት"
    },
    { "DepID":24, "DivID":2, "Department":"የኤሌክትሮ መካኒካል ኦፕሬሽን እና የጥገና ንዑስ የስራ ሂደት"},
    { "DepID":25, "DivID":2, "Department":"የማስፋፊያና እድሳት ግንባታ ቁጥጥር ኬዝ ቲም"},
    { "DepID":26, "DivID":2, "Department":"የገጠር መጠጥ ውሃ አስተዳደርና ጥገና ኬዝቲም"},
    { "DepID":27, "DivID":2, "Department":"የውሃ ጥራት ቁጥጥር እና ደህነነት ኬዝ ቲም"},
    { "DepID":31, "DivID":3, "Department":"የገቢ፤ግዥ፤ፋይናንስ፤ንብረትና አስተዳደር የስራ ሂደት"},
    { "DepID":32, "DivID":3, "Department":"የገቢ እና ፋይናንስ ንዑስ የስራ ሂደት"},
    { "DepID":33, "DivID":3, "Department":"የግዥ ንብረት አስቲዳደር ንዑስ የስራ ሂደት"},
    { "DepID":34, "DivID":3, "Department":"የኢንፎርሜሽን ቴክኖሎጅ እና የዳታ ቤዝ አስተዳደር ኬዝቲም"},
    { "DepID":35, "DivID":3, "Department":"የሰው ሀብት አስተዳደር የስራ ሂደት"},
    { "DepID":36, "DivID":3, "Department":"የዕቅድ፣ዝግጅትና የዳታ ቤዝ አስተዳደር የስራ ሂደት"},
    { "DepID":37, "DivID":3, "Department":"የደንበኞችና የህዝብ ግንኙነት የስራ ሂደት"},
    { "DepID":38, "DivID":3, "Department":"ጠቅላላለ አገልግሎት"},
    { "DepID":41, "DivID":4, "Department":"ቅርንጫፍ አንድ"},
    { "DepID":42, "DivID":4, "Department":"የደንበኞች ጉዳይና የገቢ ንዑስ የስራ ሂደት"},
    { "DepID":43, "DivID":4, "Department":"የደንበኞች መስመር ዝርጋታና የጥገና ንዑስ የስራ ሂደት"}
  ],
  "leave_quotas":[
    { "leave_type_code":"1", "quota":14},
    { "leave_type_code":"2", "quota":1},
    { "leave_type_code":"3", "quota":5},
    { "leave_type_code":"4", "quota":120},
    { "leave_type_code":"5", "quota":10},
    { "leave_type_code":"6", "quota":30},
    { "leave_type_code":"7", "quota":180},
    { "leave_type_code":"8", "quota":7}
  ],
  "salary_Scales":[
    {
      "StepId":"93444a7b-cd92-4742-807d-dd89c14be2e1",
      "GradeId":"650a1393-203d-48bf-aa69-036f0e93c799",
      "Salary":1
    }
  ],
  "master_positions":[
    { "MPosID":"1001", "MPositionName":"መካኒካል መሃንዲስ"},
    { "MPosID":"1002", "MPositionName":"ሜካኒክ"},
    { "MPosID":"1003", "MPositionName":"ሶሽዮኢኮኖሚስት"},
    { "MPosID":"1004", "MPositionName":"ረዳት ክሬን ኦፕሬተር"},
    { "MPosID":"1005", "MPositionName":"ረዳት የውሀ ጥራት ባለሙያ"},
    { "MPosID":"1006", "MPositionName":"ቀያሽ"},
    { "MPosID":"1007", "MPositionName":"ቧንቧ ባለሙያ ደረጃ 1"},
    { "MPosID":"1008", "MPositionName":"ቧንቧ ባለሙያ ደረጃ 2"},
    { "MPosID":"1009", "MPositionName":"ቧንቧ ባለሙያ ደረጃ 3"},
    { "MPosID":"1010", "MPositionName":"አጠቃላይ የኢንፎርሜሽን ቴክኖሎጂ ባለሙያ"},
    { "MPosID":"1011", "MPositionName":"ኢንቫይሮሜንታሊስት"},
    { "MPosID":"1012", "MPositionName":"ኤሌክትሪሽያን"},
    { "MPosID":"1013", "MPositionName":"ኤሌክትሪካል መሃንዲስ"},
    { "MPosID":"1014", "MPositionName":"ኤክስኪዩቲቭ ሴክሬታሪ"},
    { "MPosID":"1015", "MPositionName":"ከፍተኛ የዉሃ ክፍያ ሰነድ ዝግጅት ኦፊሰር"},
    { "MPosID":"1016", "MPositionName":"ክሬን ኦፕሬተር"},
    { "MPosID":"1017", "MPositionName":"ዋና ስራ አስኪያጅ"},
    { "MPosID":"1018", "MPositionName":"የሂሣብ ሰነድ ያዥ"},
    { "MPosID":"1019", "MPositionName":"የሂሳብ ኦፊሰር"},
    { "MPosID":"1020", "MPositionName":"የህዝብ ግንኙነት ባለሙያ"},
    { "MPosID":"1021", "MPositionName":"የህግ አገልግሎት እና የስነ-ምግባር ባለሙያ"},
    { "MPosID":"1022", "MPositionName":"የህግ አገልግሎት እና የስነ-ምግባር ኬዝ አስተባባሪ እና ባለሙያ"},
    { "MPosID":"1023", "MPositionName":"የመረጃ ዴስክ እና የአስቸኳይ ጥሪ ሰራተኛ"},
    { "MPosID":"1024", "MPositionName":"የመጠጥ ውሃ አቅርቦትና የተቋማት አስ/የስራ ሂደት መሪ"},
    { "MPosID":"1025", "MPositionName":"የመፀዳጃ ቤት አጠቃቀምና አስተዳደር ባለሙያ"},
    { "MPosID":"1026", "MPositionName":"የማስፋፊያና እድሳት ግንባታ ቁጥጥር ኬዝ ቲም አስተባባሪ"},
    { "MPosID":"1027", "MPositionName":"የማሽን/የከባድ ተሸከርካሪ ረዳት ኦፕሬተር"},
    { "MPosID":"1028", "MPositionName":"የማሽን/የከባድ ተሸከርካሪ ኦፕሬተር"},
    { "MPosID":"1029", "MPositionName":"የሠራተኛ ማህደር ዶክመንቴሽንና ፎቶ ኮፒ ሰራተኛ"},
    { "MPosID":"1030", "MPositionName":"የሰው ሀብት አስተዳደር የስራ ሂደት መሪ"},
    { "MPosID":"1031", "MPositionName":"የሰው ሀብት የምልመላና ስልጠና ባለሙያ"},
    { "MPosID":"1032", "MPositionName":"የሳኒተሪ መሀንዲስ"},
    { "MPosID":"1033", "MPositionName":"የስዌሬጅና ሳኒቴሽን መሀንዲስ"},
    { "MPosID":"1034", "MPositionName":"የቀላል ተሽከርካሪ ሾፌር"},
    { "MPosID":"1035", "MPositionName":"የቅርንጫፍ ጽ/ቤት ስራ አስኪያጅ"},
    { "MPosID":"1036", "MPositionName":"የቆጣሪ ቴክኒሺያን"},
    { "MPosID":"1037", "MPositionName":"የባጃጅ ሾፌር"},
    { "MPosID":"1038", "MPositionName":"የብየዳ ባለሙያ"},
    { "MPosID":"1039", "MPositionName":"የቧንቧ ፎርማን"},
    { "MPosID":"1040", "MPositionName":"የተሸከርካሪ ስምሪት እና እንክብካቤ ባለሙያ"},
    { "MPosID":"1041", "MPositionName":"የንብረት ምዝገባ እና ካይዘን ኦፊሰር"},
    { "MPosID":"1042", "MPositionName":"የንብረት አስተዳደር ኦፊሰር"},
    {
      "MPosID":"1043",
      "MPositionName":"የኢንፎርሜሽን ቴክኖሎጂ እና የሲስተም አስተዳደር ኬዝቲም አስተባባሪና ባለሙያ"
    },
    { "MPosID":"1044", "MPositionName":"የኤሌክትሮ መካኒካል ኦፕሬሽን እና ጥገና ንዑስ የስራ ሂደት መሪ"},
    { "MPosID":"1045", "MPositionName":"የእለት ገንዘብ ተቀባይ"},
    { "MPosID":"1046", "MPositionName":"የዕቅድ እና የበጀት ዝግጅት፣ ክትትልና ግምገማ ባለሙያ"},
    { "MPosID":"1047", "MPositionName":"የዕቅድ፣ዝግጅትና የዳታ ቤዝ አስተዳደር የስራ ሂደት መሪ"},
    { "MPosID":"1048", "MPositionName":"የኦዲዩ ቪዥዋልና ህትመት ቴክንሽያን"},
    { "MPosID":"1049", "MPositionName":"የከባድ ተሽከርካሪ ረዳት"},
    { "MPosID":"1050", "MPositionName":"የከባድ ተሽከርካሪ ሾፌር"},
    { "MPosID":"1051", "MPositionName":"የውሀ ሀብትና ተቋማት አስተዳደር ባለሙያ"},
    { "MPosID":"1052", "MPositionName":"የውሃ መሀንዲስ"},
    { "MPosID":"1053", "MPositionName":"የውሀ ምርት እና ስርጭት መሃንዲስ"},
    {
      "MPosID":"1054",
      "MPositionName":"የውሃ ምርት፤ ጥራት፤ ስርጭት እና ብክነት ቁጥጥር ንዑስ የስራ ሂደት መሪ"
    },
    { "MPosID":"1055", "MPositionName":"የውሃ ቆጣሪ ምርመራ ቴክንሽያን"},
    { "MPosID":"1056", "MPositionName":"የውሃ ቆጣሪ አንባቢ/የውሃ ክፍያ መረጃ ሰብሳቢ"},
    { "MPosID":"1057", "MPositionName":"የውሀ ቆጣሪ አንባቢዎች ቁጥጥር ኦፊሰር"},
    { "MPosID":"1058", "MPositionName":"የውሀ ብክነት ክትትል እና ቁጥጥር መሃንዲስ"},
    { "MPosID":"1059", "MPositionName":"የውሃ ጥራት ላቭራቶሪ ቴክኒሺያን"},
    { "MPosID":"1060", "MPositionName":"የውሃ ጥራት ቁጥጥር እና ደህንነት ኬዝ ቲም አስተባበሪ"},
    { "MPosID":"1061", "MPositionName":"የውሃ ጥራት ባለሙያ"},
    { "MPosID":"1062", "MPositionName":"የውስጥ ኦዲት ባለሙያ"},
    { "MPosID":"1063", "MPositionName":"የውስጥ ኦዲት ኬዝ ቲም አስተባባሪ እና ባለሙያ"},
    { "MPosID":"1064", "MPositionName":"የዉሃ ተቋማት አስተዳደር ባለሙያ"},
    { "MPosID":"1065", "MPositionName":"የዉሃ ቴክኒሻን"},
    { "MPosID":"1066", "MPositionName":"የዉሃ ክፍያ ሰነድ ሽያጭ ባለሙያ"},
    { "MPosID":"1067", "MPositionName":"የዉሃ ክፍያ ሰነድ ዝግጅት ኦፊሰር"},
    { "MPosID":"1068", "MPositionName":"የደንበኞች መስመር ዝርጋታና የጥገና ንዑስ የስራ ሂደት መሪ"},
    { "MPosID":"1069", "MPositionName":"የደንበኞች ዶክሜንተሸን ባለሙያ"},
    { "MPosID":"1070", "MPositionName":"የደንበኞች ጉዳይና የገቢ ንዑስ የስራ ሂደት መሪ"},
    { "MPosID":"1071", "MPositionName":"የደንበኞች ጉዳይ፤አገልግሎት አሰጣጥና ቅሬታ ሰሚ ባለሙያ"},
    { "MPosID":"1072", "MPositionName":"የደንበኞችና የህዝብ ግንኙነት የስራ ሂደት መሪ"},
    { "MPosID":"1073", "MPositionName":"የደንበኞችና የአገልግሎት አሰጣጥ ባለሙያ"},
    { "MPosID":"1074", "MPositionName":"የዲዛይንና ስፔስፊኬሽን መሀንዲስ"},
    { "MPosID":"1075", "MPositionName":"የገቢ እና ፋይናንስ ንዑስ የስራ ሂደት መሪ"},
    { "MPosID":"1076", "MPositionName":"የገቢ ኦፊሰር"},
    { "MPosID":"1077", "MPositionName":"የገቢ፤ግዥ፤ፋይናንስ፤ንብረትና አስተዳደር የስራ ሂደት መሪ"},
    { "MPosID":"1078", "MPositionName":"የገጠር መጠጥ ውሃ አስተዳደርና ጥገና ባለሙያና የኬዝቲም አስተባባሪ"},
    { "MPosID":"1079", "MPositionName":"የግዥ ንብረት አስተዳደር ንዑስ የስራ ሂደት መሪ"},
    { "MPosID":"1080", "MPositionName":"የግዥ ኦፊሰር"},
    { "MPosID":"1081", "MPositionName":"የጠቅላላ አገልግሎት ኃላፊ"},
    { "MPosID":"1082", "MPositionName":"የጥቅማጥቅምና ዲሲፐሊን ጉዳዮች ባለሙያ"},
    { "MPosID":"1083", "MPositionName":"የጥበቃ እና አትክልተኛ ሠራተኛ"},
    { "MPosID":"1084", "MPositionName":"የጥበቃ እና አትክልተኛ ሽፍት መሪ"},
    { "MPosID":"1085", "MPositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ"},
    { "MPosID":"1086", "MPositionName":"የጽዳትና ተላላኪ"},
    { "MPosID":"1087", "MPositionName":"የፍሳሽ መስመር ክትትልና ቁጥጥር ባለሙያ"},
    { "MPosID":"1088", "MPositionName":"የፍሳሽ ቆሻሻ አወጋገድ እና አስተዳደር ዋና የስራ ሂደት መሪ"},
    { "MPosID":"1089", "MPositionName":"የፓምፕ እና ጀኔሬተር ኦፕሬተር ሽፍት መሪ"},
    { "MPosID":"1090", "MPositionName":"ጀማሪ ውሃ መሃንዲስ"},
    { "MPosID":"1091", "MPositionName":"ገቢ ኦፊሰር/ሂሳብ ኦፊሰር"},
    { "MPosID":"1092", "MPositionName":"ገንዘብ ያዥ እና ክፍያ ኦፊሰር"},
    { "MPosID":"1093", "MPositionName":"ፓምፕ እና ጀኔሬተር ኦፕሬተር"},
    { "MPosID":"1094", "MPositionName":"ፖስተኛ"}
  ],
  "initial_salary_scale":{
    "Id":"20696b94-6c24-48b5-8001-05d69c76cf65",
    "StepId":"93444a7b-cd92-4742-807d-dd89c14be2e1",
    "GradeId":"650a1393-203d-48bf-aa69-036f0e93c799",
    "Salary":"1",
    "IsActive":true,
    "Remark":null,
    "CreateTime":"20230420114033658",
    "CreatCommandId":"d5ef3f72-e0dd-4f28-a0d5-2768cf7ecf0e",
    "UpdateTime":"20230420114033658",
    "UpdateCommandId":"d5ef3f72-e0dd-4f28-a0d5-2768cf7ecf0e"
  },
  "hr_department_positions":[
    {
      "PosID":"1069",
      "DepID":23,
      "PositionName":"የውሃ ምርት፤ ጥራት፤ ስርጭት እና ብክነት ቁጥጥር ንዑስ የስራ ሂደት መሪ",
      "MasterPosID":"1054"
    },
    {
      "PosID":"1083",
      "DepID":23,
      "PositionName":"የውሀ ምርት እና ስርጭት መሃንዲስ",
      "MasterPosID":"1053"
    },
    {
      "PosID":"1084",
      "DepID":23,
      "PositionName":"የውሀ ብክነት ክትትል እና ቁጥጥር መሃንዲስ",
      "MasterPosID":"1058"
    },
    {
      "PosID":"1085",
      "DepID":23,
      "PositionName":"የውሃ ቆጣሪ ምርመራ ቴክንሽያን",
      "MasterPosID":"1055"
    },
    {
      "PosID":"1086",
      "DepID":23,
      "PositionName":"የውሀ ቆጣሪ አንባቢዎች ቁጥጥር ኦፊሰር",
      "MasterPosID":"1057"
    },
    {
      "PosID":"1101",
      "DepID":11,
      "PositionName":"ዋና ስራ አስኪያጅ",
      "MasterPosID":"1017"
    },
    {
      "PosID":"1102",
      "DepID":11,
      "PositionName":"ኤክስኪዩቲቭ ሴክሬታሪ",
      "MasterPosID":"1014"
    },
    {
      "PosID":"1110",
      "DepID":23,
      "PositionName":"የፓምፕ እና ጀኔሬተር ኦፕሬተር ሽፍት መሪ",
      "MasterPosID":"1089"
    },
    {
      "PosID":"1201",
      "DepID":12,
      "PositionName":"የህግ አገልግሎት እና የስነ-ምግባር ኬዝ አስተባባሪ እና ባለሙያ",
      "MasterPosID":"1022"
    },
    {
      "PosID":"1202",
      "DepID":12,
      "PositionName":"የህግ አገልግሎት እና የስነ-ምግባር ባለሙያ",
      "MasterPosID":"1021"
    },
    {
      "PosID":"1301",
      "DepID":13,
      "PositionName":"የውስጥ ኦዲት ኬዝ ቲም አስተባባሪ እና ባለሙያ",
      "MasterPosID":"1063"
    },
    {
      "PosID":"1302",
      "DepID":13,
      "PositionName":"የውስጥ ኦዲት ባለሙያ",
      "MasterPosID":"1062"
    },
    {
      "PosID":"2063",
      "DepID":23,
      "PositionName":"ፓምፕ እና ጀኔሬተር ኦፕሬተር",
      "MasterPosID":"1093"
    },
    {
      "PosID":"2101",
      "DepID":21,
      "PositionName":"የመጠጥ ውሃ አቅርቦትና የተቋማት አስ/የስራ ሂደት መሪ",
      "MasterPosID":"1024"
    },
    {
      "PosID":"2102",
      "DepID":21,
      "PositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ",
      "MasterPosID":"1085"
    },
    {
      "PosID":"2201",
      "DepID":22,
      "PositionName":"የፍሳሽ ቆሻሻ አወጋገድ እና አስተዳደር ዋና የስራሂደት መሪ",
      "MasterPosID":"1088"
    },
    {
      "PosID":"2202",
      "DepID":22,
      "PositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ",
      "MasterPosID":"1085"
    },
    {
      "PosID":"2203",
      "DepID":22,
      "PositionName":"የሳኒተሪ መሃንዲስ",
      "MasterPosID":"1032"
    },
    {
      "PosID":"2204",
      "DepID":22,
      "PositionName":"የስዌሬጅና ሳኒቴሽን መሃንዲስ",
      "MasterPosID":"1033"
    },
    {
      "PosID":"2205",
      "DepID":22,
      "PositionName":"የዲዛይንና ስፔስፊኬሽን መሃንዲስ",
      "MasterPosID":"1074"
    },
    {
      "PosID":"2206",
      "DepID":22,
      "PositionName":"የመፀዳጃ ቤት አጠቃቀምና አስተዳደር ባለሙያ",
      "MasterPosID":"1025"
    },
    {
      "PosID":"2207",
      "DepID":22,
      "PositionName":"የፍሳሽ መስመር ክትትልና ቁጥጥር ባለሙያ",
      "MasterPosID":"1087"
    },
    {
      "PosID":"2208",
      "DepID":22,
      "PositionName":"ኢንቫይሮሜንታሊስት",
      "MasterPosID":"1011"
    },
    {
      "PosID":"2209",
      "DepID":22,
      "PositionName":"የማሽን/የከባድ ተሸከርካሪ ኦፕሬተር",
      "MasterPosID":"1028"
    },
    {
      "PosID":"2210",
      "DepID":22,
      "PositionName":"የማሽን/የከባድ ተሸከርካሪ ረዳት ኦፕሬተር",
      "MasterPosID":"1027"
    },
    {
      "PosID":"2401",
      "DepID":24,
      "PositionName":"የኤሌክትሮ መካኒካል ኦፕሬሽን እና ጥገና ንዑስ የስራ ሂደት መሪ",
      "MasterPosID":"1044"
    },
    {
      "PosID":"2402",
      "DepID":24,
      "PositionName":"ቧንቧ ባለሙያ ደረጃ 3",
      "MasterPosID":"1009"
    },
    { "PosID":"2403", "DepID":24, "PositionName":"የብየዳ ባለሙያ", "MasterPosID":"1038"},
    { "PosID":"2404", "DepID":24, "PositionName":"ክሬን ኦፕሬተር", "MasterPosID":"1016"},
    {
      "PosID":"2405",
      "DepID":24,
      "PositionName":"ረዳት ክሬን ኦፕሬተር",
      "MasterPosID":"1003"
    },
    {
      "PosID":"2406",
      "DepID":24,
      "PositionName":"መካኒካል መሃንዲስ",
      "MasterPosID":"1001"
    },
    { "PosID":"2407", "DepID":24, "PositionName":"ኤሌክትሪሽያን", "MasterPosID":"1012"},
    {
      "PosID":"2408",
      "DepID":24,
      "PositionName":"ኤሌክትሪካል መሃንዲስ",
      "MasterPosID":"1013"
    },
    { "PosID":"2409", "DepID":24, "PositionName":"የቧንቧ ፎርማን", "MasterPosID":"1039"},
    { "PosID":"2410", "DepID":24, "PositionName":"ሜካኒክ", "MasterPosID":"1002"},
    {
      "PosID":"2501",
      "DepID":25,
      "PositionName":"የማስፋፊያና እድሳት ግንባታ ቁጥጥር ኬዝ ቲም አስተባባሪ",
      "MasterPosID":"1026"
    },
    { "PosID":"2502", "DepID":25, "PositionName":"ሶሽዮኢኮኖሚስት", "MasterPosID":"1005"},
    { "PosID":"2503", "DepID":25, "PositionName":"ቀያሽ", "MasterPosID":"1006"},
    { "PosID":"2504", "DepID":25, "PositionName":"የውሃ መሀንዲስ", "MasterPosID":"1052"},
    {
      "PosID":"2505",
      "DepID":25,
      "PositionName":"ቧንቧ ባለሙያ ደረጃ 3",
      "MasterPosID":"1009"
    },
    {
      "PosID":"2601",
      "DepID":26,
      "PositionName":"የገጠር መጠጥ ውሃ አስተዳደርና ጥገና ባለሙያና የኬዝቲም አስተባባሪ",
      "MasterPosID":"1078"
    },
    {
      "PosID":"2602",
      "DepID":26,
      "PositionName":"የዉሃ ተቋማት አስተዳደር ባለሙያ",
      "MasterPosID":"1051"
    },
    { "PosID":"2603", "DepID":26, "PositionName":"የዉሃ ቴክኒሻን", "MasterPosID":"1065"},
    {
      "PosID":"2701",
      "DepID":27,
      "PositionName":"የውሃ ጥራት ቁጥጥር እና ደህንነት ኬዝ ቲም አስተባበሪ",
      "MasterPosID":"1060"
    },
    {
      "PosID":"2702",
      "DepID":27,
      "PositionName":"የውሀ ሀብትና ተቋማት አስተዳደር ባለሙያ",
      "MasterPosID":"1064"
    },
    {
      "PosID":"2703",
      "DepID":27,
      "PositionName":"ረዳት የውሀ ጥራት ባለሙያ",
      "MasterPosID":"1004"
    },
    {
      "PosID":"2704",
      "DepID":27,
      "PositionName":"የውሃ ጥራት ላቭራቶሪ ቴክኒሺያን",
      "MasterPosID":"1059"
    },
    {
      "PosID":"2705",
      "DepID":27,
      "PositionName":"የውሃ ጥራት ባለሙያ",
      "MasterPosID":"1061"
    },
    {
      "PosID":"3101",
      "DepID":31,
      "PositionName":"የገቢ፤ግዥ፤ፋይናንስ፤ንብረትና አስተዳደር የስራ ሂደት መሪ",
      "MasterPosID":"1077"
    },
    {
      "PosID":"3102",
      "DepID":31,
      "PositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ",
      "MasterPosID":"1085"
    },
    {
      "PosID":"3201",
      "DepID":32,
      "PositionName":"የገቢ እና ፋይናንስ ንዑስ የስራ ሂደት መሪ",
      "MasterPosID":"1075"
    },
    {
      "PosID":"3202",
      "DepID":32,
      "PositionName":"ከፍተኛ የዉሃ ክፍያ ሰነድ ዝግጅት ኦፊሰር",
      "MasterPosID":"1015"
    },
    {
      "PosID":"3203",
      "DepID":32,
      "PositionName":"የዉሃ ክፍያ ሰነድ ዝግጅት ኦፊሰር",
      "MasterPosID":"1067"
    },
    { "PosID":"3204", "DepID":32, "PositionName":"የሂሳብ ኦፊሰር", "MasterPosID":"1019"},
    {
      "PosID":"3205",
      "DepID":32,
      "PositionName":"የሂሣብ ሰነድ ያዥ",
      "MasterPosID":"1018"
    },
    {
      "PosID":"3206",
      "DepID":32,
      "PositionName":"የእለት ገንዘብ ተቀባይ",
      "MasterPosID":"1045"
    },
    { "PosID":"3207", "DepID":32, "PositionName":"የገቢ ኦፊሰር", "MasterPosID":"1076"},
    {
      "PosID":"3208",
      "DepID":32,
      "PositionName":"ገንዘብ ያዥ እና ክፍያ ኦፊሰር",
      "MasterPosID":"1092"
    },
    {
      "PosID":"3301",
      "DepID":33,
      "PositionName":"የግዥ ንብረት አስተዳደር ንዑስ የስራ ሂደት መሪ",
      "MasterPosID":"1079"
    },
    {
      "PosID":"3302",
      "DepID":33,
      "PositionName":"የንብረት ምዝገባ እና ካይዘን ኦፊሰር",
      "MasterPosID":"1041"
    },
    {
      "PosID":"3303",
      "DepID":33,
      "PositionName":"የንብረት አስተዳደር ኦፊሰር",
      "MasterPosID":"1042"
    },
    { "PosID":"3304", "DepID":33, "PositionName":"የግዥ ኦፊሰር", "MasterPosID":"1080"},
    {
      "PosID":"3401",
      "DepID":34,
      "PositionName":"የኢንፎርሜሽን ቴክኖሎጂ እና የሲስተም አስተዳደር ኬዝቲም አስተባባሪና ባለሙያ",
      "MasterPosID":"1043"
    },
    {
      "PosID":"3402",
      "DepID":34,
      "PositionName":"አጠቃላይ የኢንፎርሜሽን ቴክኖሎጂ ባለሙያ",
      "MasterPosID":"1010"
    },
    {
      "PosID":"3501",
      "DepID":35,
      "PositionName":"የሰው ሀብት አስተዳደር የስራ ሂደት መሪ",
      "MasterPosID":"1030"
    },
    {
      "PosID":"3502",
      "DepID":35,
      "PositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ",
      "MasterPosID":"1085"
    },
    {
      "PosID":"3503",
      "DepID":35,
      "PositionName":"የሰው ሀብት የምልመላና ስልጠና ባለሙያ",
      "MasterPosID":"1031"
    },
    {
      "PosID":"3504",
      "DepID":35,
      "PositionName":"የጥቅማጥቅምና ዲሲፐሊን ጉዳዮች ባለሙያ",
      "MasterPosID":"1082"
    },
    { "PosID":"3505", "DepID":35, "PositionName":"ፖስተኛ", "MasterPosID":"1094"},
    {
      "PosID":"3506",
      "DepID":35,
      "PositionName":"የሠራተኛ ማህደር ዶክመንቴሽንና ፎቶ ኮፒ ሰራተኛ",
      "MasterPosID":"1029"
    },
    {
      "PosID":"3601",
      "DepID":36,
      "PositionName":"የዕቅድ፣ዝግጅትና የዳታ ቤዝ አስተዳደር የስራ ሂደት መሪ",
      "MasterPosID":"1047"
    },
    {
      "PosID":"3602",
      "DepID":36,
      "PositionName":"የዕቅድ እና የበጀት ዝግጅት፣ ክትትልና ግምገማ ባለሙያ",
      "MasterPosID":"1046"
    },
    {
      "PosID":"3701",
      "DepID":37,
      "PositionName":"የደንበኞችና የህዝብ ግንኙነት የስራ ሂደት መሪ",
      "MasterPosID":"1072"
    },
    {
      "PosID":"3702",
      "DepID":37,
      "PositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ",
      "MasterPosID":"1085"
    },
    {
      "PosID":"3703",
      "DepID":37,
      "PositionName":"የደንበኞች ጉዳይ፤አገልግሎት አሰጣጥና ቅሬታ ሰሚ ባለሙያ",
      "MasterPosID":"1071"
    },
    {
      "PosID":"3704",
      "DepID":37,
      "PositionName":"የኦዲዩ ቪዥዋልና ህትመት ቴክንሽያን",
      "MasterPosID":"1048"
    },
    {
      "PosID":"3705",
      "DepID":37,
      "PositionName":"የህዝብ ግንኙነት ባለሙያ",
      "MasterPosID":"1020"
    },
    {
      "PosID":"3706",
      "DepID":37,
      "PositionName":"የመረጃ ዴስክ እና የአስቸኳይ ጥሪ ሰራተኛ",
      "MasterPosID":"1023"
    },
    {
      "PosID":"3707",
      "DepID":37,
      "PositionName":"የደንበኞች ጉዳይ፤አገልግሎት አሰጣጥና ቅሬታ ሰሚ ባለሙያ",
      "MasterPosID":"1071"
    },
    {
      "PosID":"3801",
      "DepID":38,
      "PositionName":"የጠቅላላ አገልግሎት ኃላፊ",
      "MasterPosID":"1081"
    },
    {
      "PosID":"3802",
      "DepID":38,
      "PositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ",
      "MasterPosID":"1085"
    },
    { "PosID":"3803", "DepID":38, "PositionName":"የጽዳትና ተላላኪ", "MasterPosID":"1086"},
    {
      "PosID":"3804",
      "DepID":38,
      "PositionName":"የተሸከርካሪ ስምሪት እና እንክብካቤ ባለሙያ",
      "MasterPosID":"1040"
    },
    {
      "PosID":"3805",
      "DepID":38,
      "PositionName":"የጥበቃ እና አትክልተኛ ሽፍት መሪ",
      "MasterPosID":"1084"
    },
    {
      "PosID":"3806",
      "DepID":38,
      "PositionName":"የጥበቃ እና አትክልተኛ ሠራተኛ",
      "MasterPosID":"1083"
    },
    { "PosID":"3807", "DepID":38, "PositionName":"የባጃጅ ሾፌር", "MasterPosID":"1037"},
    {
      "PosID":"3808",
      "DepID":38,
      "PositionName":"የከባድ ተሽከርካሪ ሾፌር",
      "MasterPosID":"1050"
    },
    {
      "PosID":"3809",
      "DepID":38,
      "PositionName":"የከባድ ተሽከርካሪ ረዳት",
      "MasterPosID":"1049"
    },
    {
      "PosID":"3810",
      "DepID":38,
      "PositionName":"የቀላል ተሽከርካሪ ሾፌር",
      "MasterPosID":"1034"
    },
    {
      "PosID":"4101",
      "DepID":41,
      "PositionName":"የቅርንጫፍ ጽ/ቤት ስራ አስኪያጅ",
      "MasterPosID":"1035"
    },
    {
      "PosID":"4102",
      "DepID":41,
      "PositionName":"የጽህፈትና ቢሮ አስተዳደር ባለሙያ",
      "MasterPosID":"1085"
    },
    {
      "PosID":"4103",
      "DepID":41,
      "PositionName":"የንብረት አስተዳደር ኦፊሰር",
      "MasterPosID":"1042"
    },
    {
      "PosID":"4104",
      "DepID":41,
      "PositionName":"የጥበቃ እና አትክልተኛ ሠራተኛ",
      "MasterPosID":"1083"
    },
    { "PosID":"4105", "DepID":41, "PositionName":"የጽዳትና ተላላኪ", "MasterPosID":"1086"},
    {
      "PosID":"4201",
      "DepID":42,
      "PositionName":"የደንበኞች ጉዳይና የገቢ ንዑስ የስራ ሂደት መሪ",
      "MasterPosID":"1070"
    },
    {
      "PosID":"4202",
      "DepID":42,
      "PositionName":"የዉሃ ክፍያ ሰነድ ሽያጭ ባለሙያ",
      "MasterPosID":"1066"
    },
    {
      "PosID":"4203",
      "DepID":42,
      "PositionName":"የውሃ ቆጣሪ አንባቢ/የውሃ ክፍያ መረጃ ሰብሳቢ",
      "MasterPosID":"1056"
    },
    {
      "PosID":"4204",
      "DepID":42,
      "PositionName":"የደንበኞች ዶክሜንተሸን ባለሙያ",
      "MasterPosID":"1069"
    },
    {
      "PosID":"4205",
      "DepID":42,
      "PositionName":"የደንበኞችና የአገልግሎት አሰጣጥ ባለሙያ",
      "MasterPosID":"1073"
    },
    {
      "PosID":"4206",
      "DepID":42,
      "PositionName":"ገቢ ኦፊሰር/ሂሳብ ኦፊሰር",
      "MasterPosID":"1091"
    },
    {
      "PosID":"4207",
      "DepID":42,
      "PositionName":"የእለት ገንዘብ ተቀባይ",
      "MasterPosID":"1045"
    },
    {
      "PosID":"4301",
      "DepID":43,
      "PositionName":"የደንበኞች መስመር ዝርጋታና የጥገና ንዑስ የስራ ሂደት መሪ",
      "MasterPosID":"1068"
    },
    {
      "PosID":"4302",
      "DepID":43,
      "PositionName":"ጀማሪ ውሃ መሃንዲስ",
      "MasterPosID":"1090"
    },
    {
      "PosID":"4303",
      "DepID":43,
      "PositionName":"ቧንቧ ባለሙያ ደረጃ 1",
      "MasterPosID":"1007"
    },
    {
      "PosID":"4304",
      "DepID":43,
      "PositionName":"ቧንቧ ባለሙያ ደረጃ 2",
      "MasterPosID":"1008"
    },
    {
      "PosID":"4305",
      "DepID":43,
      "PositionName":"ቧንቧ ባለሙያ ደረጃ 3",
      "MasterPosID":"1009"
    },
    {
      "PosID":"4306",
      "DepID":43,
      "PositionName":"የቆጣሪ ቴክኒሺያን",
      "MasterPosID":"1036"
    }
  ]
}
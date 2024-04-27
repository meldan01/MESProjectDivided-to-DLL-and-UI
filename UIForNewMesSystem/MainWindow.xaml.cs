using NewMasApp.ExternalComponents;
using NewMasApp.WorkEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MachineEntity = NewMasApp.WorkEntities.Machine;
using PartEntity = NewMasApp.WorkEntities.Part;
using WorkOrderEntity = NewMasApp.WorkEntities.WorkOrder;
using WorkEntities = NewMasApp.WorkEntities;


namespace UIForNewMesSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Logger m_logsInstance;
        public static DBConnectionManager m_dbConnection;
        private static bool m_machineSent = false, m_partSent = false, m_workOrderSent = false;
        public MainWindow()
        {
            InitializeComponent();
            startExternalComponents();
        }


        #region General

        private static void startExternalComponents()
        {
            m_logsInstance = Logger.getInstance();
            m_dbConnection = DBConnectionManager.getInstance();
        }

        /// <summary>
        /// isUpdatedSetter - A flag setter funcion 
        /// </summary>
        /// <param name="isSuccessfulUpdate"></param>
        #endregion General

        #region Machine
        private void saveMachine_Click(object sender, RoutedEventArgs e)
        {
            txtMachineSaveWarning.Visibility = Visibility.Collapsed;
            if (areAllMachineFieldsValid())
            {
                if (!WorkEntities.Machine.machineExists(txtMachineName.Text))
                {
                    sendMachineToDB();
                }
                else
                {
                    setMachineUIMessage("Machine name already exist.", Brushes.Red, Visibility.Visible);
                }
            }
            else
            {
                setMachineUIMessage("Invalid input detected. Please ensure all fields" +
                    " are filled correctly to proceed.", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// setMachineMessage - Indicates user about state of the system
        /// </summary>
        /// <param name="message"></param>
        /// <param name="foreground"></param>
        /// <param name="visibility"></param>
        private void setMachineUIMessage(string message, Brush foreground, Visibility visibility)
        {
            txtMachineSaveWarning.Text = message;
            txtMachineSaveWarning.Foreground = foreground;
            txtMachineSaveWarning.Visibility = visibility;
        }

        /// <summary>
        /// startSuccessfulMachineProtocol - Method that run after all check are done when sabe btn pressed
        /// </summary>
        private void sendMachineToDB()
        {
            WorkEntities.Machine newMachine = new WorkEntities.Machine(dpDateOfCreation.SelectedDate.Value, txtCreatorID.Text, txtLanguageCode.Text, txtMachineName.Text);
            newMachine.sendMachineToDB();
            logAndUserUiMachineMessage();
        }

        private void logAndUserUiMachineMessage()
        {
            if (m_machineSent)
            {
                m_machineSent = false;
                m_logsInstance.Log("Debug" + $" Machine number {txtMachineName.Text} successfully sent to the DB");
                setMachineUIMessage($"Machine {txtMachineName.Text} successfully sent to DB", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                m_logsInstance.Log("Error" + $" - Machine {txtMachineName.Text} Could not be created due to unknown Error in the constructor");
                setMachineUIMessage($"Machine {txtMachineName.Text} could not be added to the DB", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// CheckMachineFields - check that the properties of machine 
        /// are properly filled before sending to DB
        /// </summary>
        /// <returns></returns>
        private bool areAllMachineFieldsValid()
        {
            return machineNameValidation(txtMachineName.Text) & machineCreatorIdValidation(txtCreatorID.Text) &
                 machineLanguageCodeValidation(txtLanguageCode.Text) & machineDpValidation(dpDateOfCreation.SelectedDate);
        }

        /// <summary>
        /// getMachinesInfo_Click - gets all the machines info from the DB
        /// and show it in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getMachinesInfo_Click(object sender, RoutedEventArgs e)
        {
            txtMachineSaveWarning.Visibility = Visibility.Collapsed;
            txtMachineMessage.Text = WorkEntities.Machine.getMachinesInfo();
            setMachineUIMessage("Information updated", Brushes.LightGreen, Visibility.Visible);
        }

        /// <summary>
        /// updateMachine_Click - Updates the current machine .
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateMachine_Click(object sender, RoutedEventArgs e)
        {
            if (!validateMachineUIFields())
                return;
            if (MachineEntity.updateMachine(txtMachineName.Text, dpDateOfCreation.SelectedDate, txtCreatorID.Text, txtLanguageCode.Text))
            {
                setMachineUIMessage($"Machine {txtMachineName.Text} successfully updated", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                setMachineUIMessage($"Could not update machine {txtMachineName.Text}", Brushes.Red, Visibility.Visible);
            }
        }

        private bool validateMachineUIFields()
        {
            if (string.IsNullOrEmpty(txtMachineName.Text))
            {
                setMachineUIMessage($"Please type a valid machine name to update", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!WorkEntities.Machine.machineExists(txtMachineName.Text))
            {
                setMachineUIMessage($"Machine {txtMachineName.Text} not exist in the Database", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (isAllMachineFieldsEmpty())
            {
                setMachineUIMessage($"Please fill at least on field to update.", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!MachineEntity.isUpdateMachineFieldsValid(txtCreatorID.Text))
            {
                setMachineUIMessage($"Creator ID should be a valid ID(9 digits)", Brushes.Red, Visibility.Visible);
                return false;
            }
            return true;
        }

        /// <summary>
        /// machineNameValidation - machineName validation
        /// </summary>
        /// <returns></returns>
        private bool machineNameValidation(string machineName)
        {
            if (string.IsNullOrEmpty(machineName))
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineCreatorIdValidation - validation to the creator ID
        /// </summary>
        /// <returns></returns>
        private bool machineCreatorIdValidation(string creatorID)
        {
            if (string.IsNullOrEmpty(creatorID) || (creatorID.Length != 9) || !creatorID.All(char.IsDigit))
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineLanguageCodeValidation - validation of the language code of machine
        /// </summary>
        /// <returns></returns>
        private bool machineLanguageCodeValidation(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) ||
                (!languageCode.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineDpValidation - validation of the datePicker
        /// </summary>
        /// <returns></returns>
        private bool machineDpValidation(DateTime? creationDate)
        {
            if (creationDate == null)
                return false;
            if (!creationDate.HasValue)
                return false;
            else
                return true;
        }

        private bool isAllMachineFieldsEmpty()
        {
            return string.IsNullOrEmpty(txtCreatorID.Text) &
             string.IsNullOrEmpty(txtLanguageCode.Text) &
             !dpDateOfCreation.SelectedDate.HasValue;
        }

        /// <summary>
        /// deleteMachine_Click - deletes the current machine.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteMachine_Click(object sender, RoutedEventArgs e)
        {
            if (!userValidationDeleteMachine())
                return;
            if (MachineEntity.deleteMachine(txtMachineName.Text))
            {
                setMachineUIMessage($"Machine {txtMachineName.Text} deleted successfully", Brushes.LightGreen, Visibility.Visible);
                m_logsInstance.Log($"Debug - Machine {txtMachineName.Text} deleted successfully.");
                DeleteAllMachineRelatedWorkOrders(txtMachineName.Text);
                return;
            }
            else
            {
                setMachineUIMessage($"Could not delete machine {txtMachineName.Text} from the Database", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log($"Error - machine {txtMachineName.Text} not deleted.");
                return;
            }
        }

        private bool userValidationDeleteMachine()
        {
            if (string.IsNullOrEmpty(txtMachineName.Text))
            {
                setMachineUIMessage("Please type machine name to delete", Brushes.Red, Visibility.Visible);
                return false;
            }
            if (!DBConnectionManager.machineExists(txtMachineName.Text))
            {
                setMachineUIMessage($"Could not find machine {txtMachineName.Text} in the Database", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log($"Debug - machine {txtMachineName.Text} not found, therefore it will not be deleted");
                return false;
            }
            return true;
        }

        /// <summary>
        /// DeleteAllMachineRelatedWorkOrders - deletes all the work orders that related to the current deleted machine
        /// </summary>
        /// <param name="text"></param>
        private void DeleteAllMachineRelatedWorkOrders(string text)
        {
            if (!MachineEntity.DeleteOrdersByMachineName(txtMachineName.Text))
            {
                m_logsInstance.Log($"Debug - No work orders that related to {txtMachineName.Text} deleted.");
            }
        }
        #endregion Machine


        #region Part


        /// <summary>
        /// SavePart_Click - Saves new part to the DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePart_Click(object sender, RoutedEventArgs e)
        {
            txtPartSaveWarning.Visibility = Visibility.Collapsed;

            if (areAllPartFieldsValid())
            {
                if (!WorkEntities.Part.partExists(txtCatalogID.Text))
                {
                    sendPartToDB();
                }
                else
                {
                    setPartUIMessage($"Catalog ID number {txtCatalogID.Text} already exist.", Brushes.Red, Visibility.Visible);
                }
            }
            else
            {
                setPartUIMessage("Invalid input detected. Please ensure all fields are filled correctly to proceed."
                    , Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// updatePart_Click - If the the part exists, updates it according to the fields that are not null 
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updatePart_Click(object sender, RoutedEventArgs e)
        {
            if (!PartUpdateFieldsValidation())
                return;
            if (PartEntity.updatePart(txtCatalogID.Text, txtItemDescription.Text, dpPartDateOfCreation.SelectedDate, txtPartCreatorID.Text, txtPartLanguageCode.Text))
                successfullPartUpdateLogUIMessage();
            else
                failurePartUpdateLogUIMessage();
        }

        private void failurePartUpdateLogUIMessage()
        {
            setPartUIMessage($"Could not update part {txtCatalogID.Text}."
    , Brushes.Red, Visibility.Visible);
            m_logsInstance.Log($"Error - Could not update part {txtCatalogID.Text}.");
        }

        private void successfullPartUpdateLogUIMessage()
        {
            setPartUIMessage($"Part {txtCatalogID.Text} successfully updated."
    , Brushes.LightGreen, Visibility.Visible);
            m_logsInstance.Log($"Debug - Part {txtCatalogID.Text} successfully updated.");
        }

        /// <summary>
        /// PartUpdateFieldsValidation - validates the fields of the part before updating
        /// </summary>
        /// <returns></returns>
        private bool PartUpdateFieldsValidation()
        {
            if (string.IsNullOrEmpty(txtCatalogID.Text))
            {
                setPartUIMessage($"Please enter catalog ID."
                , Brushes.Red, Visibility.Visible);
                return false;
            }
            if (PartEntity.partExists(txtCatalogID.Text))
            {
                if (isAllPartFieldsEmpty())
                {
                    setPartUIMessage($"Please fill at least one field to update."
                    , Brushes.Red, Visibility.Visible);
                    return false;
                }
                if (descriptionLengthValidation())
                    return true;
                else
                {
                    setPartUIMessage($"Description of part {txtCatalogID.Text} is too short."
                        , Brushes.Red, Visibility.Visible);
                    return false;
                }
            }
            else
            {
                setPartUIMessage($"Part {txtCatalogID.Text} not exist in the DataBase."
                  , Brushes.Red, Visibility.Visible);
                return false;
            }
        }

        private bool isAllPartFieldsEmpty()
        {
            return
            string.IsNullOrEmpty(txtItemDescription.Text) &
            string.IsNullOrEmpty(txtPartCreatorID.Text) &
            string.IsNullOrEmpty(txtPartLanguageCode.Text) &
            !dpPartDateOfCreation.SelectedDate.HasValue;
        }

        /// <summary>
        /// SetPartMessage - indicates the user about the system status by a print to the UI
        /// </summary>
        /// <param name="message"></param>
        /// <param name="foreground"></param>
        /// <param name="visibility"></param>
        private void setPartUIMessage(string message, Brush foreground, Visibility visibility)
        {
            txtPartSaveWarning.Text = message;
            txtPartSaveWarning.Foreground = foreground;
            txtPartSaveWarning.Visibility = visibility;
        }

        /// <summary>
        /// descriptionLengthValidation - makes sure that the user added some proper description
        /// </summary>
        /// <returns></returns>
        private bool descriptionLengthValidation()
        {
            if (!string.IsNullOrEmpty(txtItemDescription.Text))
            {
                if (txtItemDescription.Text.Length < 50)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// startSuccessfulPartProtocol - After the validations start running the add to DB validation
        /// </summary>
        private void sendPartToDB()
        {
            WorkEntities.Part newPart = new WorkEntities.Part(dpPartDateOfCreation.SelectedDate.Value, txtPartCreatorID.Text, txtPartLanguageCode.Text, txtCatalogID.Text, txtItemDescription.Text);
            m_partSent = newPart.sendPartToDB();
            logAndUIPartMessage();
        }

        private void logAndUIPartMessage()
        {
            if (m_partSent)
            {
                m_partSent = false;
                m_logsInstance.Log("Debug" + $"Part {txtCatalogID.Text} successfully sent to the DB");
                setPartUIMessage($"Part {txtCatalogID.Text} successfully sent to DB", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                m_logsInstance.Log("Error" + $"Part {txtCatalogID.Text} Could not be created due to unknown Error in the constructor");
                setPartUIMessage($"Error - Part {txtCatalogID.Text} could not be added to the DB", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// areAllPartFieldsValid - Validates fields in the UI are properly filled
        /// </summary>
        /// <returns></returns>
        private bool areAllPartFieldsValid()
        {
            return partCatalogIdValidation(txtCatalogID.Text) & partDescriptionValidation(txtItemDescription.Text) &
                partDpValidation(dpPartDateOfCreation.SelectedDate) & partCreatorIdValidation(txtPartCreatorID.Text) & partLanguageCodeValidation(txtPartLanguageCode.Text);
        }

        /// <summary>
        /// partCatalogIdValidation - Validates the catalog ID field
        /// </summary>
        /// <returns></returns>
        private bool partCatalogIdValidation(string catalogID)
        {
            if (string.IsNullOrEmpty(catalogID) ||
                (!catalogID.All(char.IsDigit)))

                return false;
            else
                return true;
        }

        /// <summary>
        /// partDescriptionValidation - Validates the description field
        /// </summary>
        /// <returns></returns>
        private bool partDescriptionValidation(string description)
        {
            if (description.Length < 50)//minimal description
                return false;
            else
                return true;
        }

        /// <summary>
        /// partDpValidation - Validates the part datePicker field
        /// </summary>
        /// <returns></returns>
        private bool partDpValidation(DateTime? selectedDate)
        {
            if (!selectedDate.HasValue)
                return false;
            else
                return true;
        }

        /// <summary>
        /// partCreatorIdValidation - Validates the creator field
        /// </summary>
        /// <returns></returns>
        private bool partCreatorIdValidation(string creatorID)
        {
            if (string.IsNullOrEmpty(creatorID) || (creatorID.Length != 9) || !creatorID.All(char.IsDigit))
                return false;
            else
                return true;
        }

        /// <summary>
        /// partLanguageCodeValidation - Validates the language code field
        /// </summary>
        /// <returns></returns>
        private bool partLanguageCodeValidation(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) ||
                (!languageCode.All(char.IsDigit)))
                return false;
            else
                return true;
        }


        /// <summary>
        /// getPartsInfo_Click - Gets the Part information from the DB and show it in the textBlock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getPartsInfo_Click(object sender, RoutedEventArgs e)
        {
            txtPartSaveWarning.Visibility = Visibility.Collapsed;
            txtPartMessage.Text = PartEntity.getPartsInfo();
            setPartUIMessage("Information updated", Brushes.LightGreen, Visibility.Visible);
        }


        /// <summary>
        /// deletePart_Click - Deletes the current part 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deletePart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCatalogID.Text))
            {
                setPartUIMessage("Please type a valid catalog ID to delete", Brushes.Red, Visibility.Visible);
                return;
            }
            if (PartEntity.partExists(txtCatalogID.Text))
            {
                if (PartEntity.deletePart(txtCatalogID.Text))
                {
                    setPartUIMessage($"Part {txtCatalogID.Text} successfully deleted,", Brushes.LightGreen, Visibility.Visible);
                    m_logsInstance.Log($"Debug - part {txtCatalogID.Text} successfully deleted");
                    if (PartEntity.deletePart(txtCatalogID.Text))
                    {
                        m_logsInstance.Log($"Debug - deleted all orders related to {txtCatalogID.Text} successfully.");
                    }
                }
            }
            else
            {
                setPartUIMessage($"Part {txtCatalogID.Text} not found in the DataBase", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log($"Debug - Part {txtCatalogID.Text} not found in the DataBase");
            }
        }

        #endregion Part

        #region WorkOrder

        /// <summary>
        /// saveWorkOrder_Click - Saves the current workOrder to the DB 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            txtWorkOrderSaveWarning.Visibility = Visibility.Collapsed;
            if (areAllWorkOrderFieldsValid())
            {
                if (!DBConnectionManager.orderNumberExists(txtOrderNumber.Text))
                {
                    if (!validateMachinePartOrder())
                    {
                        setWorkOrderUIMessage($"Machine or part not exist.", Brushes.Red, Visibility.Visible);
                        return;
                    }
                    startSuccessfulWorkOrderProtocol();
                }
                else
                {
                    setWorkOrderUIMessage($"Work order number {txtOrderNumber.Text} already exist.", Brushes.Red, Visibility.Visible);
                }
            }
            else
            {
                setWorkOrderUIMessage("Invalid input detected. Please ensure all fields are filled correctly to proceed.", Brushes.Red, Visibility.Visible);
            }
        }

        private bool validateMachinePartOrder()
        {
            if (!checkCatalogIdAndMachineNameExist(textMachineName.Text, txtWorkOrderCatalogID.Text))
            {
                setWorkOrderUIMessage("Machine or Part not exist", Brushes.Red, Visibility.Visible);
                m_logsInstance.Log("Debug" + $" - Machine or Part not exist or order number exist, therefore order {txtOrderNumber.Text}  will not be added to DB");
                return false;
            }
            return true;
        }

        /// <summary>
        /// startSuccessfulWorkOrderProtocol - After all the validations 
        /// </summary>
        private void startSuccessfulWorkOrderProtocol()
        {
            WorkEntities.WorkOrder newWorkOrder = new WorkEntities.WorkOrder(dpDateOfCreationWorkOrder.SelectedDate.Value, txtOrderIDCreatorID.Text, txtWorkOrderLanguageCode.Text, txtOrderNumber.Text, txtWorkOrderCatalogID.Text, textMachineName.Text, txtAmountToProduce.Text);
            m_workOrderSent = newWorkOrder.sendWorkOrderToDB();
            if (m_workOrderSent)
            {
                m_workOrderSent = false;
                m_logsInstance.Log("Debug" + $"Work order number {txtOrderNumber.Text} successfully sent to the DB");
                setWorkOrderUIMessage("Work order successfully sent to DB", Brushes.LightGreen, Visibility.Visible);
            }
            else
            {
                m_logsInstance.Log("Error" + $" - Work order {txtOrderNumber.Text} Could not be created due to unknown Error in the constructor");
                setPartUIMessage($"Error - Work order {txtOrderNumber.Text} could not be added to the DB", Brushes.Red, Visibility.Visible);
            }
        }

        /// <summary>
        /// checkCatalogIdAndMachineNameExist - Check that both the machine and part exist in the DB
        /// </summary>
        /// <param name="machineName"></param>
        /// <param name="catalogID"></param>
        /// <returns></returns>
        private bool checkCatalogIdAndMachineNameExist(string machineName, string catalogID)
        {
            return DBConnectionManager.machineExists(machineName) & DBConnectionManager.catalogIDExists(catalogID);
        }

        /// <summary>
        /// The next function returns true if all the fields in work order section
        /// filled properly, otherwise false
        /// </summary>
        /// <returns></returns>
        private bool areAllWorkOrderFieldsValid()
        {
            return orderNumberValidation(txtOrderNumber.Text) & catalogIDValidation(txtWorkOrderCatalogID.Text) & machineNameOrderValidation(textMachineName.Text) &
                   amountToProduceValidation(txtAmountToProduce.Text) & dpWorkOrderValidation(dpDateOfCreationWorkOrder.SelectedDate) & creatorIDWorkOrderValidation(txtOrderIDCreatorID.Text) &
                   languageCodeWorkOrderValidation(txtWorkOrderLanguageCode.Text);
        }

        /// <summary>
        /// orderNumberValidation - Validates the order number field
        /// </summary>
        /// <returns></returns>
        private bool orderNumberValidation(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber) ||
                (!orderNumber.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// catalogIDValidation - validates the catalog ID field
        /// </summary>
        /// <returns></returns>
        private bool catalogIDValidation(string catalogID)
        {
            if (string.IsNullOrEmpty(catalogID) ||
                (!catalogID.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// amountToProduceValidation -  Validates the quantity field
        /// </summary>
        /// <returns></returns>
        private bool amountToProduceValidation(string quantity)
        {
            if (string.IsNullOrEmpty(quantity) ||
                (!quantity.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// machineNameOrderValidation - Validates the machine name field
        /// </summary>
        /// <returns></returns>
        private bool machineNameOrderValidation(string machineName)
        {
            if (string.IsNullOrEmpty(machineName) || machineName.Length > 50)
                return false;
            else
                return true;
        }

        /// <summary>
        /// dpWorkOrderValidation - Validates datepicker field
        /// </summary>
        /// <returns></returns>
        private bool dpWorkOrderValidation(DateTime? creationDate)
        {
            if ((!creationDate.HasValue))
                return false;
            else
                return true;
        }

        /// <summary>
        /// creatorIDWorkOrderValidation -  Validates the creator ID field
        /// </summary>
        /// <returns></returns>
        private bool creatorIDWorkOrderValidation(string creatorID)
        {
            if ((creatorID.Length != 9) ||
                (!creatorID.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// languageCodeWorkOrderValidation - Validates the language code field
        /// </summary>
        /// <returns></returns>
        private bool languageCodeWorkOrderValidation(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) ||
                (!languageCode.All(char.IsDigit)))
                return false;
            else
                return true;
        }

        /// <summary>
        /// getWorkOrdersInfo_Click - Gets and show in the UI the entire DB workOrders table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getWorkOrdersInfo_Click(object sender, RoutedEventArgs e)
        {
            txtWorkOrderSaveWarning.Visibility = Visibility.Collapsed;
            txtWorkOrderMessage.Text = WorkOrderEntity.getOrdersInfo();
            setWorkOrderUIMessage("Information updated", Brushes.LightGreen, Visibility.Visible);
        }

        /// <summary>
        /// deleteWorkOrder_Click - Deletes the current workOrder 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!orderNumberValidation(txtOrderNumber.Text))
            {
                setWorkOrderUIMessage("Please type a valid order number to delete", Brushes.Red, Visibility.Visible);
                return;
            }
            if (!WorkOrderEntity.orderExists(txtOrderNumber.Text))
            {
                setWorkOrderUIMessage("Order number not found in the Database", Brushes.Red, Visibility.Visible);
                return;
            }
            if (WorkOrderEntity.deleteWorkOrderByOrderNumber(txtOrderNumber.Text))
            {
                setWorkOrderUIMessage($"Order number {txtOrderNumber.Text} deleted successfully", Brushes.LightGreen, Visibility.Visible);
                m_logsInstance.Log($"Order number {txtOrderNumber.Text} deleted successfully");
            }
            else
                setWorkOrderUIMessage($"Could not delete order number {txtOrderNumber.Text}", Brushes.Red, Visibility.Visible);
        }

        /// <summary>
        /// updateWorkOrder_Click - Updates the current workOrder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtOrderNumber.Text))
            {
                setWorkOrderUIMessage("Please type order number", Brushes.Red, Visibility.Visible);
                return;
            }
            if (isAllOrderUIFieldsEmpty())
            {
                setWorkOrderUIMessage("Please fill at list one field to update", Brushes.Red, Visibility.Visible);
                return;
            }
            try
            {
                bool updateSucceeded = WorkOrderEntity.updateWorkOrder(txtOrderNumber.Text, txtWorkOrderCatalogID.Text, textMachineName.Text, txtAmountToProduce.Text, dpDateOfCreationWorkOrder.SelectedDate,
                    txtOrderIDCreatorID.Text, txtWorkOrderLanguageCode.Text);
                if (updateSucceeded)
                    setWorkOrderUIMessage($"Order {txtOrderNumber.Text} successfully updated", Brushes.LightGreen, Visibility.Visible);
                else
                {
                    setWorkOrderUIMessage($"Could not update order {txtOrderNumber.Text}", Brushes.Red, Visibility.Visible);
                    m_logsInstance.Log("Debug - " + $"Could not update order {txtOrderNumber.Text}");
                }
            }
            catch (Exception ex)
            {
                m_logsInstance.Log("EXCEPTION" + ex.Message);
            }
        }

        /// <summary>
        /// isAllOrderUIFieldsEmpty - return true if all the order fields are null or empty
        /// in case of datepicker if it empty 
        /// </summary>
        /// <returns></returns>
        private bool isAllOrderUIFieldsEmpty()
        {
            return string.IsNullOrEmpty(txtWorkOrderCatalogID.Text) &
             string.IsNullOrEmpty(textMachineName.Text) &
             string.IsNullOrEmpty(txtAmountToProduce.Text) &
             string.IsNullOrEmpty(txtOrderIDCreatorID.Text) &
             string.IsNullOrEmpty(txtWorkOrderLanguageCode.Text) &
             !dpDateOfCreationWorkOrder.SelectedDate.HasValue;
        }


        /// <summary>
        /// setWorkOrderMessage - Set messages to the UI
        /// </summary>
        /// <param name="message"></param>
        /// <param name="foreground"></param>
        /// <param name="visibility"></param>
        private void setWorkOrderUIMessage(string message, Brush foreground, Visibility visibility)
        {
            txtWorkOrderSaveWarning.Text = message;
            txtWorkOrderSaveWarning.Foreground = foreground;
            txtWorkOrderSaveWarning.Visibility = visibility;
        }
        #endregion WorkOrder
    }
}

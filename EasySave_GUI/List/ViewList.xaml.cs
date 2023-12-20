using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EasySave_GUI.ListView
{
    public partial class ViewList : UserControl
    {
        // La collection de sauvegardes (à remplacer par le type réel que vous utilisez)
        private ObservableCollection<BackupItem> backupItems = new ObservableCollection<BackupItem>();

        public ViewList()
        {
            InitializeComponent();
            // Autre code d'initialisation ici
            // Vous voudrez peut-être définir le DataContext sur votre ViewModel
            // DataContext = new YourViewModel();

            // Attachez le gestionnaire d'événements au clic sur le bouton Ajouter
            addBtn.Click += AddBtn_Click;

            // Assurez-vous que votre ListView est lié à la collection de sauvegardes
            ItemListView.ItemsSource = backupItems;

            // Attachez le gestionnaire d'événements à la sélection d'un élément dans la ListBox
            ItemListView.SelectionChanged += ItemListView_SelectionChanged;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            // Accédez aux valeurs des zones de texte et de la zone de liste déroulante
            string nom = nameText.Text;
            string source = SourceText.Text;
            string cible = TargetText.Text;
            string typeDeSauvegarde = sauvegarde.Text;

            // Vérifiez si toutes les données sont renseignées
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(cible) || typeDeSauvegarde == "Type of save" )
            {
                MessageBox.Show("Veuillez renseigner toutes les données.");
                return; // Ne rien faire si les données sont manquantes 
            }
            // Vérifiez si le nom est déjà présent
            if (backupItems.Any(item => item.Nom == nom))
            {
                MessageBox.Show("Veuillez renseigner un autre nom. Ce nom existe déjà.");
                return;
            }
            // Créez un nouvel objet BackupItem (à remplacer par le type réel que vous utilisez)
            BackupItem newBackup = new BackupItem
            {
                Nom = nom,
                Source = source,
                Cible = cible,
                TypeDeSauvegarde = typeDeSauvegarde
            };

            // Ajoutez le nouvel objet à la collection de sauvegardes
            backupItems.Add(newBackup);

            // Rafraîchissez la vue (peut ne pas être nécessaire, dépend de votre liaison de données)
            CollectionViewSource.GetDefaultView(ItemListView.ItemsSource).Refresh();

            // Vous pouvez ensuite transmettre ces valeurs à votre ViewModel ou effectuer d'autres actions nécessaires.
            // YourViewModel.AddBackup(nom, source, cible, typeDeSauvegarde);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Utilisez OpenFileDialog pour simuler la sélection de dossier
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                FileName = "Select a folder",
                Filter = "Folders|*.none",
                CheckFileExists = false,
                CheckPathExists = true
            };

            // Afficher la boîte de dialogue de sélection de fichier
            bool? result = openFileDialog.ShowDialog();

            // Vérifier si l'utilisateur a sélectionné un fichier
            if (result == true)
            {
                // Extraire le chemin du fichier sélectionné (qui est en fait un dossier)
                string folderPath = Path.GetDirectoryName(openFileDialog.FileName);

                // Mettre à jour le chemin dans le TextBox
                SourceText.Text = folderPath;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Utilisez OpenFileDialog pour simuler la sélection de dossier
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                FileName = "Select a folder",
                Filter = "Folders|*.none",
                CheckFileExists = false,
                CheckPathExists = true
            };

            // Afficher la boîte de dialogue de sélection de fichier
            bool? result = openFileDialog.ShowDialog();

            // Vérifier si l'utilisateur a sélectionné un fichier
            if (result == true)
            {
                // Extraire le chemin du fichier sélectionné (qui est en fait un dossier)
                string folderPath = Path.GetDirectoryName(openFileDialog.FileName);

                // Mettre à jour le chemin dans le TextBox
                TargetText.Text = folderPath;
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Récupérez la liste des éléments sélectionnés dans la ListBox
            var selectedBackups = ItemListView.SelectedItems.Cast<BackupItem>().ToList();

            // Parcourez la liste et supprimez chaque élément de la collection backupItems
            foreach (var selectedBackup in selectedBackups)
            {
                backupItems.Remove(selectedBackup);
            }
        }



        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Obtenez l'élément sélectionné dans la ListBox
            BackupItem selectedBackup = (BackupItem)ItemListView.SelectedItem;

            // Vérifiez si un élément est sélectionné
            if (selectedBackup != null)
            {
                // Affichez les données de l'élément sélectionné dans les TextBox associés
                nameText.Text = selectedBackup.Nom;
                SourceText.Text = selectedBackup.Source;
                TargetText.Text = selectedBackup.Cible;
                sauvegarde.Text = selectedBackup.TypeDeSauvegarde;
            }
        }

        private void SendBtn_Click_1(object sender, RoutedEventArgs e)
        {
            // Obtenez l'élément sélectionné dans la ListBox
            BackupItem selectedBackup = (BackupItem)ItemListView.SelectedItem;

            // Vérifiez si un élément est sélectionné
            if (selectedBackup != null)
            {
                // Obtenez les nouvelles valeurs des TextBox
                string newNom = nameText.Text;
                string newSource = SourceText.Text;
                string newCible = TargetText.Text;
                string newTypeDeSauvegarde = sauvegarde.Text;

                // Vérifiez si le nouveau nom existe déjà dans la liste
                if (backupItems.Any(item => item.Nom == newNom && item != selectedBackup))
                {
                    MessageBox.Show("Le nom existe déjà. Veuillez choisir un autre nom.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Mettez à jour les valeurs de l'élément sélectionné
                selectedBackup.Nom = newNom;
                selectedBackup.Source = newSource;
                selectedBackup.Cible = newCible;
                selectedBackup.TypeDeSauvegarde = newTypeDeSauvegarde;

                // Rafraîchissez la vue
                CollectionViewSource.GetDefaultView(ItemListView.ItemsSource).Refresh();
            }
        }

    }

    // Exemple de classe BackupItem (à adapter à votre structure de données réelle)
    public class BackupItem
    {
        public string Nom { get; set; }
        public string Source { get; set; }
        public string Cible { get; set; }
        public string TypeDeSauvegarde { get; set; }
    }
}

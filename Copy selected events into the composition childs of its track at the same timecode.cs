/**
 * Ce script copie le(s) évenement(s) vidéo sélectionnés dans le projet dans toutes les pistes "enfant de composition"
 * de la piste de l'évènement sélectionné.
 **/

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using ScriptPortal.Vegas;

public class EntryPoint
{
    public void FromVegas(Vegas vegas)
    {
        //On récupère tous les évènements sélectionnés par l'utilisateur
        //Une entrée de la liste = un ensemble d'évènements sélectionnés sur une même piste
        List<VideoTrack> listePistesEvenementsSelectionne = new List<VideoTrack>();
        List<List<VideoEvent>> listeEvenementsSelectionne = new List<List<VideoEvent>>();

        bool onEteTrouvesEvenementsDansPiste = false;
        int i = 0;

        //On récupère les évènements vidéo sélectionnés de toutes les pistes en contenant
		foreach (Track trkItem in vegas.Project.Tracks)
		{
            if(trkItem.MediaType == MediaType.Video)
            {
                VideoTrack videoTrack = (VideoTrack) trkItem;

                foreach (VideoEvent evtItem in videoTrack.Events)
                {
                    if(evtItem.Selected == true)
                    {
                        if(onEteTrouvesEvenementsDansPiste == false)
                        {
                            listePistesEvenementsSelectionne.Add(videoTrack);
                            listeEvenementsSelectionne.Add(new List<VideoEvent>());
                            onEteTrouvesEvenementsDansPiste = true;
                        }

                        listeEvenementsSelectionne[i].Add(evtItem);
                    }
                }
            }

            if(onEteTrouvesEvenementsDansPiste == true)
            {
                i = i + 1;
            }
            onEteTrouvesEvenementsDansPiste = false;
		}

        //Si on a trouvé des évènements sélectionnés
        if(listeEvenementsSelectionne.Count > 0)
        {
            i = 0;

            while(i < listeEvenementsSelectionne.Count)
            {
                VideoTrack pisteEvementsSelectionnes = listePistesEvenementsSelectionne[i];

                //Si la piste parcourue actuellement contient des enfants de composition, on copie les évènements sélectionnés
                //dans ces enfants de compositions (au même timecode, on est pas des sauvages).
                if(pisteEvementsSelectionnes.IsCompositingParent == true)
                {
                    //Le niveau de composition d'une piste est le nombre de pères qu'elle a
                    Int32 niveauDeCompositionPisteEvementsSelectionnes = pisteEvementsSelectionnes.CompositeNestingLevel;

                    //Numéro de la piste
                    Int32 indexPisteEvementsSelectionnes = pisteEvementsSelectionnes.Index;

                    int j = 1;
                    VideoTrack pisteParcourue;
                    bool tousLesFilsParcourus = false;

                    while(i < vegas.Project.Tracks.Count && tousLesFilsParcourus == false)
                    {
                        pisteParcourue = (VideoTrack) vegas.Project.Tracks[indexPisteEvementsSelectionnes + j];

                        //On regarde si la piste actuelle est bien un fils direct de la piste sélectionnés
                        if((pisteParcourue.MediaType == MediaType.Video)
                        && (pisteParcourue.CompositeNestingLevel == (niveauDeCompositionPisteEvementsSelectionnes + 1)))
                        {
                            //Copie des évènements sélectionnés
                            foreach (VideoEvent evtItem in listeEvenementsSelectionne[i])
                            {
                                evtItem.Copy(vegas.Project.Tracks[indexPisteEvementsSelectionnes + j], evtItem.Start);
                            }
                        }
                        //Sinon, on a bien parcouru tous les fils
                        else
                        {
                            if(pisteParcourue.CompositeNestingLevel <= (niveauDeCompositionPisteEvementsSelectionnes + 1))
                            {
                                tousLesFilsParcourus = true;
                            }
                        }

                        j = j + 1;
                    }
                }

                i = i + 1;
            }
        }
	}
}

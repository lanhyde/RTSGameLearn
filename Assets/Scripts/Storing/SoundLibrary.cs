using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Storing
{
    [System.Serializable]
    public class SoundsCategory
    {
        public string categoryName = "Category Name";
        public List<AudioClip> audioClips = new List<AudioClip>();
        public List<SoundsSubCategory> subCategories = new List<SoundsSubCategory>();
    }

    [System.Serializable]
    public class SoundsSubCategory
    {
        public string categoryName = "Sub Category name";
        public List<AudioClip> audioClips= new List<AudioClip>();
    }

    public class SoundAttribute: PropertyAttribute{}

    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "RTS/Sound Library")]
    public class SoundLibrary: ScriptableObject
    {
        public List<SoundsCategory> soundsCategories = new List<SoundsCategory>();

        public List<string> GetSoundPaths(List<SoundsCategory> fromCategories)
        {
            var soundsList = new List<string>();

            soundsList.Add("None or missing");

            for(int i = 0; i < fromCategories.Count; ++i)
            {
                var category = fromCategories[i];
                string categoryName = category.categoryName;

                for(int k = 0; k < category.audioClips.Count; ++k)
                {
                    if(!fromCategories[i].audioClips[k])
                    {
                        continue;
                    }
                    soundsList.Add(categoryName + "/" + fromCategories[i].audioClips[k].name);
                }
                var subSounds = GetSoundsFromSubCategories(category.subCategories);
                for(int k = 0; i < subSounds.Count; ++k)
                {
                    subSounds[k] = categoryName + "/" + subSounds[k];
                }
                soundsList.AddRange(subSounds);
            }
            return soundsList;
        }

        public List<string> GetSoundsFromSubCategories(List<SoundsSubCategory> subCategories)
        {
            var soundsList = new List<string>();

            for(int i = 0; i < subCategories.Count; ++i)
            {
                for(int k = 0; k < subCategories[i].audioClips.Count; ++k)
                {
                    soundsList.Add(subCategories[i].categoryName + "/" + subCategories[i].audioClips[k].name);
                }
            }
            return soundsList;
        }
        /// <summary>
        /// This method returns sound Audioclip by its path in SoundCategories. It searches for first match in subcategories if any of them exists, otherwise in parent directory.
        /// </summary>
        public AudioClip GetSoundByPath(string path)
        {
            string[] pathSplitted = path.Split(new char[] {'/'}, System.StringSplitOptions.RemoveEmptyEntries);

            SoundsCategory parentCategory = null;

            for(int k = 0; k < soundsCategories.Count; ++k)
            {
                if(soundsCategories[k].categoryName == pathSplitted[0])
                {
                    parentCategory = soundsCategories[k];
                    break;
                }
            }

            SoundsSubCategory subCategory = null;
            for(int i = 1; i < pathSplitted.Length; ++i)
            {
                // while (i != index of last path element (sound name)) searching for category
                if(i < pathSplitted.Length - 1)
                {
                    for(int k = 0; k < parentCategory.subCategories.Count; ++k)
                    {
                        if(parentCategory.subCategories[k].categoryName == pathSplitted[i])
                        {
                            subCategory = parentCategory.subCategories[k];
                            break;
                        }
                    }
                }
                // otherwise searching for sound name
                else
                {
                    // if we found an subcategory, get sound from it
                    if(subCategory != null)
                    {
                        for(int k = 0; k < subCategory.audioClips.Count; ++k)
                        {
                            if(subCategory.audioClips[k].name == pathSplitted[i])
                            {
                                return subCategory.audioClips[k];
                            }
                        }
                    }
                    // otherwise get sound from parent catalog
                    else
                    {
                        for(int k = 0; k < parentCategory.audioClips.Count; ++k)
                        {
                            if(parentCategory.audioClips[k].name == pathSplitted[i])
                            {
                                return parentCategory.audioClips[k];
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
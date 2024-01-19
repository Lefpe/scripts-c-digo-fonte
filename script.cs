using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
// Esta classe gerencia os botões, cenário, áudio e lógica geral do jogo.
public class ButtonsGame : MonoBehaviour {
    //botões do game //
    public Button[] _buttons;
    public Color neutralColor;
    public Color disableColor;
    // cenario do game //
    public GameObject tipTemplate;
    public GameObject[] _resBlocks;
    public GameObject character;
    public GameObject popupIcon;
    //help audio do game //
    public List<AudioClip> audioClipsList = new List<AudioClip>();
    //textos aparentes do game //
    public Text _text;
    public Text _text2;
    
   //caixa de palavras//
    public Word[] words;

    public GameObject popUpBox;

    private int _wordIndex = 0;
    private int _wordPositionIndex = 0;
    private int _syllableIndex = 0;
    private int? _response;
    private List<(GameObject, Vector3)?> _movementsData = new List<(GameObject, Vector3)?>();
    private List<GameObject> _cachedGameObjects = new List<GameObject>();
    private GameObject _currentTip;
    public List<SkinConfig> skinConfigList;
    private int _currentSkinIndex = 0;
    private int _currentAudioIndex = 0;


    //get current word//palavras atuais do jogo//
    public Word GetCurrentWord() => words[_wordIndex];
    //retorna a silaba atual da palavra//
    public String currentSyllable {
        get => GetCurrentWord().syllables[_syllableIndex];

    }
    //bloco de resposta atual destacado//
    public GameObject currentResBlock {
        get => _resBlocks[_syllableIndex];
    }
//palavra a ser inserida//
    void Start() {
        //inicia o carergamento da palavra atual//
        LoadCurrentWord();
    }
    
//animação de proxima fase//
    public void NextLevel() {
        var anim = popUpBox.GetComponent<Animation>();
        anim.Play("popupFadeout");
        popUpBox.SetActive(false);
        _wordIndex++;

        if (_wordIndex < words.Length) {
            LoadCurrentWord();
        } else {
            SceneManager.LoadScene("Stage2");
        }



        //TODO: change character//

    }
    //transição dos blocos//
    void Update() {
    //como movimento de objetos//
        foreach (var movementData in _movementsData.ToArray()) {
            if (movementData.HasValue) {
                (GameObject movingBlock, Vector3 targetPosition) = movementData!.Value;
                if (movingBlock == null) {
                    _movementsData.Remove(movementData);
                } else {
                    movingBlock.transform.position = Vector3.Lerp(movingBlock.transform.position, targetPosition, 5 * Time.deltaTime);
                    if (movingBlock.transform.position == targetPosition) {
                        _movementsData.Remove(movementData);
                    }
                }
            }
        }

    }

    private void LoadCurrentSyllables() {
        var word = GetCurrentWord();
        for (int i = 0; i < _buttons.Length; i++) {
            var syllable = word.syllableBlock[_syllableIndex].syllables[i];
            _buttons[i].GetComponentInChildren<Text>().text = syllable;
        }

        TriggerTip();
    }

    private void LoadCurrentWord() {
        _response = null;
        _syllableIndex = 0;
        ClearCachedGameObjects();

        if (_wordIndex < words.Length) {
            var word = GetCurrentWord();
            _text.text = word.first;
            _text2.text = word.second;

            LoadCurrentSyllables();
        } else {
            SceneManager.LoadScene("Stage2");
        }
    }


    private bool VerifyResponse() {
        var word = GetCurrentWord();
        var answer = word.syllables[_syllableIndex];
        var syllables = word.syllableBlock[_syllableIndex].syllables;
        return String.Equals(syllables[_response ?? 0], answer);
    }

    //Verify response order//
    // Método para lidar com a resposta do jogador a uma sílaba.
    public void OnResponse(int pos) {
        _response = pos;
        var word = GetCurrentWord();
        if (_syllableIndex + 1 == word.syllables.Length) {
            if (VerifyResponse()) {
                SendToResBlock(_buttons[pos].gameObject);
                if ((_wordIndex + 1) % 6 == 0) {
                    popUpBox.SetActive(true);

                    var anim = popUpBox.GetComponent<Animation>();
                    anim.Play("fadein");

                    _currentSkinIndex++;

                    AudioSource.PlayClipAtPoint(audioClipsList[_currentAudioIndex], Camera.main.transform.position);
                    _currentAudioIndex++;
                    //skin level up do personagem//
                    popupIcon.GetComponent<Image>().sprite = skinConfigList[_currentSkinIndex].icon;
                    character.GetComponent<Image>().sprite = skinConfigList[_currentSkinIndex].skin;


                    //TODO: Show stars
                } else {
                    _wordIndex++;
                    AudioSource.PlayClipAtPoint(audioClipsList[_currentAudioIndex], Camera.main.transform.position);
                    _currentAudioIndex++;
                    Invoke("LoadCurrentWord", 2f);
                }
            } else {
                // error
            }
        } else {
            if (VerifyResponse()) {
                SendToResBlock(_buttons[pos].gameObject);
                _syllableIndex++;
                LoadCurrentSyllables();
            } else {
                // error
            }
        }
    }
// Método para exibir dicas visuais para o jogador//
    public void TriggerTip() {
        NormalizeButtons();
        if (_wordIndex < 9) {
            foreach (var button in _buttons) {
                print(button.GetComponentInChildren<Text>().text);
                print(currentSyllable);
                if (button.GetComponentInChildren<Text>().text.Equals(currentSyllable, StringComparison.OrdinalIgnoreCase)) {
                    button.GetComponent<Image>().color = neutralColor;
                    var newTip = Instantiate(tipTemplate, button.transform);
                    _currentTip = newTip;
                } else {
                    button.GetComponent<Image>().color = disableColor;
                }
            }
        } else if (_wordIndex < 19) {
            foreach (var button in _buttons) {
                print(button.GetComponentInChildren<Text>().text);
                print(currentSyllable);
                if (button.GetComponentInChildren<Text>().text.Equals(currentSyllable, StringComparison.OrdinalIgnoreCase)) {
                    button.GetComponent<Image>().color = neutralColor;

                    var newTip = Instantiate(tipTemplate, button.transform);


                    _currentTip = newTip;

                    _currentTip.GetComponent<Animation>().Play();
                }
            }
        }
    }
 // Método para normalizar as cores dos botõe//
    public void NormalizeButtons() {
        foreach (var button in _buttons) {
            button.GetComponent<Image>().color = neutralColor;
        }
    }
    // Método para mover um bloco para o bloco de resposta//
    public void SendToResBlock(GameObject block) {
        Destroy(_currentTip);
        var newBlock = Instantiate(block, currentResBlock.transform);
        newBlock.transform.position = block.transform.position;
        _cachedGameObjects.Add(newBlock);
        _movementsData.Add((newBlock, currentResBlock.transform.position));
    }
// Método para limpar os objetos em cache.
    public void ClearCachedGameObjects() {
        foreach (var cachedGameObject in _cachedGameObjects) {
            GameObject.Destroy(cachedGameObject);
        }
        _cachedGameObjects.Clear();
    }
}
// Classe que representa uma lista de sílabas.
[Serializable]
public class SyllableList {
    public List<String> syllables = new List<String>();
}
//atualização da skin do personagem//
[Serializable]
public class SkinConfig {
    public Sprite icon;
    public Sprite skin;
}
//familia silabica//
public class SyllableFamily {
    public string letter;
    public List<string> syllableFamily;

    public SyllableFamily(string letter, List<string> syllableFamily) {
        this.letter = letter;
        this.syllableFamily = syllableFamily;
    }
}

[Serializable]
public class Word {
  //lista de vogais//
    private List<String> vowels = new List<String> { "a", "e", "i", "o", "u" };

    public string[] syllables;
    //blocos silabicos//
    public List<SyllableList> syllableBlock {
        get {
            List<SyllableList> syllableLists = new List<SyllableList>();
            foreach (var syllable in syllables) {
                SyllableList syllableList = new SyllableList();
                foreach (var vowel in vowels) {
                    syllableList.syllables.Add((syllable[0] + vowel).ToUpper());
                }
                syllableLists.Add(syllableList);
            }
            return syllableLists;
        }
    }
// Método para obter a primeira sílaba da palavra//
    public string first {
        get => syllables.GetValue(0).ToString();
    }
    // Método para obter a segunda sílaba da palavra//
    public string second {
        get => syllables.GetValue(1).ToString();
    }
}
// Classe que lida com o sistema de pop-up no jogo//
public class PopUpSystem : MonoBehaviour {
    //pop up system//
    public GameObject popUpBox;
    public Animator animator;
    public TMP_Text popUpText;
//exibir os pop-up//
    public void PopUp() {
        popUpBox.SetActive(true);

    }
}

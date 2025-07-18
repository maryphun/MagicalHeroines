using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SimpleLocalization.Scripts;
using System.Linq;

/// <summary>
/// �v���C���[�̃Q�[���i���͑S�Ă����ɋL�^����
/// �Z�[�u���[�h�͂��̍\���̂�ۑ�������ǂ��Ƃ����F��
/// </summary>
[System.Serializable]
public struct PlayerData
{
    public int currentStage;             //< ���X�e�[�W��
    public int currentMoney;             //< ����
    public int currentResourcesPoint;    //< �����|�C���g
    public SideQuestData sideQuestData;  //< �e���̌x���x���L�^
    public List<Character> characters;     //< �����Ă���L�����N�^�[
    public FormationSlotData[] formationCharacters; //< �p�[�e�B�[�Ґ�
    public List<ItemDefine> inventory;   //< �����A�C�e��
    public List<EquipmentData> equipment;   //< ��������
    public List<HomeDialogue> homeDialogue;   //< �z�[���V�[���̃Z���t���Ǘ�����
    public int formationSlotUnlocked;    //< ������ꂽ�X���b�g
    public TutorialData tutorialData;    //< �`���[�g���A�������������Ǘ�
    public List<Record> records;         //< �N�H�L�^

    /// <summary>
    /// DLC �p�Z�[�u�f�[�^
    /// </summary>
    public int currentDLCStage;          //< ���X�e�[�W��
}

/// <summary>
/// �����`���[�g���A�����Ǘ�
/// </summary>
[System.Serializable]
public struct TutorialData
{
    public bool worldscene;
    public bool formationPanel;
    public bool characterbuildingPanel;
    public bool trainPanel;

    public TutorialData(bool value)
    {
        worldscene = value;
        formationPanel = value;
        characterbuildingPanel = value;
        trainPanel = value;
    }
}

public class ProgressManager : SingletonMonoBehaviour<ProgressManager>
{
    public bool IsInitialized { get { return isInitialized; } }
    [SerializeField] bool isInitialized = false;

    public PlayerData PlayerData { get { return playerData; } }
    [SerializeField] PlayerData playerData;

#if DEBUG_MODE
    bool isDebugModeInitialized = false;
#endif

    /// <summary>
    /// �Q�[���i����������Ԃɂ���
    /// </summary>
    public void InitializeProgress()
    {
        playerData = new PlayerData();

        playerData.currentStage = 1; // �����X�e�[�W (�`���[�g���A��)
        playerData.currentDLCStage = 1;
        playerData.currentMoney = 500;
        playerData.currentResourcesPoint = 0;
        playerData.sideQuestData = new SideQuestData(1, 1, 1);
        playerData.characters = new List<Character>();
        playerData.formationCharacters = new FormationSlotData[5];
        playerData.inventory = new List<ItemDefine>();
        playerData.equipment = new List<EquipmentData>();
        playerData.homeDialogue = new List<HomeDialogue>();
        playerData.formationSlotUnlocked = 2;
        playerData.tutorialData = new TutorialData(false);
        playerData.records = new List<Record>();

        // �����z�[���V�[���L����
        HomeDialogue no5 = Resources.Load<HomeDialogue>("HomeDialogue/No5");
        playerData.homeDialogue.Add(no5);
        //Resources.UnloadAsset(no5);

        // �����L���� 
        PlayerCharacterDefine battler = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/1.Battler");
        AddPlayerCharacter(battler);
        //Resources.UnloadAsset(battler);

        PlayerCharacterDefine tentacle = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/2.TentacleMan");
        AddPlayerCharacter(tentacle);
        //Resources.UnloadAsset(tentacle);

        PlayerCharacterDefine clone = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/3.Clone");
        AddPlayerCharacter(clone);
        //Resources.UnloadAsset(clone);

        // �����L�����������I�Ƀp�[�e�B�[�ɕғ�����
        for (int i = 0; i < playerData.formationCharacters.Length; i++)
        {
            if (i < playerData.formationSlotUnlocked)
            {
                playerData.formationCharacters[i] = new FormationSlotData(playerData.characters[i].characterData.characterID, true);
            }
            else
            {
                playerData.formationCharacters[i] = new FormationSlotData(-1, false);
            }
        }

        // ����������������
        var randomizer = new System.Random();
        int seed = randomizer.Next(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed + System.Environment.TickCount);

        // �t���O�X�V
        isInitialized = true;
    }

    public void ApplyLoadedData(PlayerData data)
    {
        playerData = data;

        // ����������������
        var randomizer = new System.Random();
        int seed = randomizer.Next(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed + System.Environment.TickCount);

        // �t���O�X�V
        isInitialized = true;

#if DEBUG_MODE
        DLCManager.isDLCEnabled = true;
#endif
    }

    /// <summary>
    /// ���݂̃Q�[���i�s�󋵂��擾
    /// </summary>
    public int GetCurrentStageProgress()
    {
        return playerData.currentStage;
    }

    /// <summary>
    /// ���݂̃Q�[���i�s�󋵂��擾
    /// </summary>
    public int GetCurrentDLCStageProgress()
    {
        return playerData.currentDLCStage;
    }

    /// <summary>
    /// ���݂̃Q�[���i�s�󋵂��擾
    /// </summary>
    public bool IsGameEnded()
    {
        const int EndingProgress = 21;
        return playerData.currentStage >= EndingProgress;
    }

    /// <summary>
    /// ���݂̃Q�[���i�s�󋵂��擾
    /// </summary>
    public bool IsDLCEnded()
    {
        const int EndingProgress = 9;
        return playerData.currentDLCStage >= EndingProgress;
    }

    /// <summary>
    /// �X�g�[���[�i�s
    /// </summary>
    public void StageProgress(int value = 1)
    {
        playerData.currentStage += value;
        PlayerPrefsManager.UpdateCurrentProgress(playerData.currentStage);
    }

    /// <summary>
    /// DLC�X�g�[���[�i�s
    /// </summary>
    public void DLCStageProgress(int value = 1)
    {
        playerData.currentDLCStage += value;
    }

    /// <summary>
    /// �L�����N�^�[�������X�V
    /// </summary>
    public void UpdateCharacterData(List<Character> characters)
    {
        playerData.characters = characters;
    }

    /// <summary>
    /// �����Ă��钇�Ԃ̃��X�g���擾
    /// </summary>
    public List<Character> GetAllCharacter(bool originalReference = false, bool includeDLCCharacters = false)
    {
        if (originalReference)
        {
            if (includeDLCCharacters)
            {
                return playerData.characters;
            }
            else
            {
                return playerData.characters.Where(x => !x.characterData.isDLCCharacter).ToList();
            }
        }
        else
        {
            List<Character> characterListCopy = new List<Character>(playerData.characters);
            characterListCopy = characterListCopy.Where(x => !x.characterData.isDLCCharacter || includeDLCCharacters).ToList();
            return characterListCopy;
        }
    }

    // �Y���̃L�����������Ă��邩
    public bool HasCharacter(int characterID, bool mustBeUsable = true)
    {
        if (mustBeUsable)
        {
            return playerData.characters.Any(x => x.characterData.characterID == characterID && (!x.characterData.is_heroin || x.is_corrupted));
        }
        return playerData.characters.Any(x => x.characterData.characterID == characterID);
    }

    /// <summary>
    /// �퓬�I����ɃL������HP��MP���f�[�^�ɓ�����
    /// </summary>
    public void UpdateCharacterByBattler(int characterID, Battler battler)
    {
        var character = playerData.characters.Find(item => item.characterData.characterID == characterID);

        character.current_hp = Mathf.Max(battler.current_hp, 1); // �Œ�1�_�ɂ���
        character.current_mp = battler.current_mp;
    }

    /// <summary>
    /// CharacterID ����L�����N�^�[���擾����
    /// </summary>
    /// <returns></returns>
    public Character GetCharacterByID(int characterID)
    {
        List<Character> characterListCopy = new List<Character>(playerData.characters);

        return characterListCopy.Find(item => item.characterData.characterID == characterID);
    }

    /// <summary>
    /// Character�� ����L�����N�^�[�����[�h����
    /// </summary>
    public Character LoadCharacter(string name)
    {
        return ConvertCharacterDefine(Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/" + name));
    }

    /// <summary>
    /// �g���钇�ԃL�����̃��X�g���擾
    /// </summary>
    public List<Character> GetAllUsableCharacter(bool includeDLCCharacter)
    {
        List<Character> usableCharacter = playerData.characters.Where(data => (data.is_corrupted || !data.characterData.is_heroin) && (includeDLCCharacter || !data.characterData.isDLCCharacter)).ToList();

        return usableCharacter;
    }

    /// <summary>
    /// �V�������Ԓǉ�
    /// </summary>
    public Character AddPlayerCharacter(PlayerCharacterDefine newCharacter)
    {
        Character obj = ConvertCharacterDefine(newCharacter);
        playerData.characters.Add(obj);
        return obj;
    }

    private Character ConvertCharacterDefine(PlayerCharacterDefine newCharacter)
    {
        var obj = new Character();

        obj.pathName = newCharacter.name;

        obj.localizedName = LocalizationManager.Localize(newCharacter.detail.nameID);
        obj.characterData = newCharacter.detail;
        obj.battler = newCharacter.battler;
        obj.current_level = newCharacter.detail.starting_level;
        obj.current_maxHp = newCharacter.detail.base_hp;
        obj.current_maxMp = newCharacter.detail.base_mp;
        obj.current_hp = newCharacter.detail.base_hp;
        obj.current_mp = newCharacter.detail.base_mp;
        obj.current_attack = newCharacter.detail.base_attack;
        obj.current_defense = newCharacter.detail.base_defense;
        obj.current_speed = newCharacter.detail.base_speed;
        obj.is_corrupted = !(newCharacter.detail.is_heroin); // �q���C���L�����͂Ƃ肠�����g�p�ł��Ȃ�

        return obj;
    }

    public void RelocalizeCharactersName()
    {
        foreach (Character character in playerData.characters)
        {
            character.localizedName = LocalizationManager.Localize(character.characterData.nameID);
        }
    }

    /// <summary>
    /// ���ݎ����ʂ��擾
    /// </summary>
    public int GetCurrentMoney()
    {
        return playerData.currentMoney;
    }

    /// <summary>
    /// �����ʂ�ύX
    /// </summary>
    public void SetMoney(int newValue)
    {
        playerData.currentMoney = Mathf.Max(newValue, 0);
    }

    /// <summary>
    /// ���ݎ����ʂ��擾
    /// </summary>
    public int GetCurrentResearchPoint()
    {
        return playerData.currentResourcesPoint;
    }

    /// <summary>
    /// �����ʂ�ύX
    /// </summary>
    public void SetResearchPoint(int newValue)
    {
        playerData.currentResourcesPoint = Mathf.Max(newValue, 0);
    }

    /// <summary>
    /// �p�[�e�B�[�Ґ��ő吔���擾
    /// </summary>
    public int GetUnlockedFormationCount()
    {
        return playerData.formationSlotUnlocked;
    }

    /// <summary>
    /// �p�[�e�B�[�Ґ��ő吔�𑝉�
    /// </summary>
    public void UnlockedFormationCount()
    {
        playerData.formationSlotUnlocked ++;
    }

    /// <summary>
    /// �o���p�[�e�B�[�擾
    /// </summary>
    public FormationSlotData[] GetFormationParty(bool originalReference = false)
    {
        if (originalReference)
        {
            return playerData.formationCharacters;
        }
        else
        {
            FormationSlotData[] partyListCopy = (FormationSlotData[])playerData.formationCharacters.Clone();
            return partyListCopy;
        }
    }

    /// <summary>
    /// ����̃L�������t�H�[���[�V�����Ґ�����Ă��邩�ǂ���
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsCharacterInFormationParty(int characterID)
    {
        var rtn = playerData.formationCharacters.FirstOrDefault(c => c.characterID == characterID);

        return rtn != null;
    }

    /// <summary>
    /// �o���p�[�e�B�[�ݒ�
    /// </summary>
    public void SetFormationParty(FormationSlotData[] characters)
    {
        playerData.formationCharacters = characters;
    }


    /// <summary>
    /// �o���p�[�e�B�[�擾
    /// </summary>
    public int GetCharacterNumberInFormationParty()
    {
        int count = 0;

        for (int i = 0; i < playerData.formationCharacters.Length; i++)
        {
            if (playerData.formationCharacters[i].isFilled)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// �����Ă���A�C�e���̃��X�g���擾
    /// </summary>
    public List<ItemDefine> GetItemList(bool originalReference = false)
    {
        if (originalReference)
        {
            return playerData.inventory;
        }
        else
        {
            List<ItemDefine> itemListCopy = new List<ItemDefine>(playerData.inventory);
            return itemListCopy;
        }
    }

    /// <summary>
    /// �A�C�e��������ɒB���Ă��邩
    /// </summary>
    public bool IsItemFull()
    {
        const int MaxItemSlot = 48;
        return playerData.inventory.Count >= MaxItemSlot;
    }

    /// <summary>
    /// �C���x���g���̎c��̋�
    /// </summary>
    public int GetInventorySlotLeft()
    {
        const int MaxItemSlot = 48;
        return Mathf.Max(MaxItemSlot - playerData.inventory.Count, 0);
    }

    /// <summary>
    /// �A�C�e���������Ă��邩���`�F�b�N
    /// </summary>
    public bool PlayerHasItem(ItemDefine item)
    {
        if (playerData.inventory != null)
        {
            return playerData.inventory.Any((x) => x.pathName == item.pathName);
        }
        return false;
    }

    /// <summary>
    /// �C���x���g�����X�V
    /// </summary>
    public void SetItemList(List<ItemDefine> newList)
    {
        playerData.inventory = newList;
    }

    /// <summary>
    /// �A�C�e���l��
    /// </summary>
    public void AddItemToInventory(ItemDefine item)
    {
        playerData.inventory.Add(item);
    }

    /// <summary>
    /// �A�C�e���l��
    /// </summary>
    public void RemoveItemFromInventory(ItemDefine item)
    {
        playerData.inventory.Remove(item);
    }

    /// <summary>
    /// �z�[���䎌�L�����N�^�[���擾
    /// </summary>
    public List<HomeDialogue> GetHomeCharacter(bool DLC_Character_Only = false) 
    {
        if (DLC_Character_Only) return playerData.homeDialogue.Where(x => x.isDLCCharacter).ToList();
        return playerData.homeDialogue;
    }

    /// <summary>
    /// �z�[���䎌�L�����N�^�[��ǉ�
    /// </summary>
    public void AddHomeCharacter(HomeDialogue data)
    {
        playerData.homeDialogue.Add(data);
    }

    /// <summary>
    /// �z�[���䎌�L�����N�^�[��r��
    /// </summary>
    public void RemoveHomeCharacter(string dataPathName)
    {
        for (int i = 0; i < playerData.homeDialogue.Count; i++)
        {
            if (string.Equals(playerData.homeDialogue[i].pathName, dataPathName))
            {
                playerData.homeDialogue.RemoveAt(i);
                return;
            }
        }

        Debug.LogWarning("�폜���悤�Ƃ��Ă���z�[���L���������݂��Ă��Ȃ�");
    }

    /// <summary>
    /// ��������肷��
    /// </summary>
    public void AddNewEquipment(EquipmentDefine data)
    {
        EquipmentData newEquipment = new EquipmentData(data);
        playerData.equipment.Add(newEquipment);
    }

    /// <summary>
    /// �w��̑����A�C�e���𑕔�����, �Z�[�u�f�[�^�̓s����CharacterID�Ƃ��ăf�[�^���c���A�����ƃL������R����
    /// </summary>
    public void ApplyEquipmentToCharacter(EquipmentDefine data, int characterID)
    {
        for (int i = 0; i < playerData.equipment.Count; i++)
        {
            if (playerData.equipment[i].data.pathName == data.pathName)
            {
                playerData.equipment[i].equipingCharacterID = characterID;
            }
        }
    }

    /// <summary>
    /// �������O��
    /// </summary>
    public void UnapplyEquipment(string name)
    {
        for (int i = 0; i < playerData.equipment.Count; i++)
        {
            if (playerData.equipment[i].data.pathName == name)
            {
                playerData.equipment[i].equipingCharacterID = -1;
            }
        }
    }

    public List<EquipmentData> GetEquipmentData(bool originalReference = false)
    {
        if (originalReference)
        {
            return playerData.equipment;
        }
        else
        {
            List<EquipmentData> copy = new List<EquipmentData>(playerData.equipment);
            return copy;
        }
    }

    /// <summary>
    /// �����������Ă��邩���`�F�b�N
    /// </summary>
    public bool PlayerHasEquipment(EquipmentDefine equipment)
    {
        if (playerData.equipment != null)
        {
            return playerData.equipment.Any((x) => x.data.pathName == equipment.pathName);
        }
        return false;
    }

    /// <summary>
    /// ���̃L�������������Ă���A�C�e�����擾
    /// </summary>
    public bool GetCharacterEquipment(int characterID, ref EquipmentDefine result)
    {
        for (int i = 0; i < playerData.equipment.Count; i++)
        {
            if (playerData.equipment[i].equipingCharacterID == characterID)
            {
                result = playerData.equipment[i].data;
                return true; 
            }
        }

        return false;
    }

    /// <summary>
    /// �x���x��ς���
    /// </summary>
    public void SetSideQuestData(int foodQuest, int bankQuest, int researchQuest)
    {
        const int MaxAlertLevel = 5;
        playerData.sideQuestData = new SideQuestData(Mathf.Clamp(foodQuest, 1, MaxAlertLevel), Mathf.Clamp(bankQuest, 1, MaxAlertLevel), Mathf.Clamp(researchQuest, 1, MaxAlertLevel));
    }

    public SideQuestData GetSideQuestData()
    {
        return playerData.sideQuestData;
    }

    public void SetTutorialData(TutorialData data)
    {
        playerData.tutorialData = data;
    }

    public TutorialData GetTutorialData()
    {
        return playerData.tutorialData;
    }

    // �N�H�L�^�ǉ�
    public void AddNewRecord(string recordNameID, string recordNovelID)
    {
        Record temp = new Record(recordNameID, recordNovelID);
        playerData.records.Add(temp);
    }

    // �܂��ʒm���͂��Ă��Ȃ��N�H�L�^�����邩
    public bool HasUnnotifiedRecord()
    {
        if (playerData.Equals(default(PlayerData))) return false;

        return playerData.records.Any(x => x.isNotified == false);
    }

    // �܂��ʒm���͂��Ă��Ȃ��N�H�L�^�����邩
    public Record GetUnnotifiedRecord()
    {
        return playerData.records.First(x => x.isNotified == false);
    }

    // �N�H�L�^���X�g���l��
    public List<Record> GetRecordsList()
    {
        if (playerData.records == null) return new List<Record>();

        return playerData.records;
    }

    public void RecordNotified(string recordNameID)
    {
        var copy = playerData.records;

        foreach (var record in copy)
        {
            if (record.recordNameID == recordNameID && record.isNotified == false)
            {
                record.isNotified = true;
            }
        }

        // paste
        playerData.records = copy;
    }
    public void RecordChecked(string recordNameID)
    {
        var copy = playerData.records;

        foreach (var record in copy)
        {
            if (record.recordNameID == recordNameID && record.isChecked == false)
            {
                record.isChecked = true;
            }
        }

        // paste
        playerData.records = copy;
    }

#if DEBUG_MODE
    public void DebugModeInitialize(bool addEnemy = false)
    {
        if (isDebugModeInitialized) return;
        ProgressManager.Instance.InitializeProgress();
        playerData.currentStage = 2; // (�`���[�g���A�����X�L�b�v)
        ProgressManager.Instance.SetMoney(Random.Range(200, 9999));
        ProgressManager.Instance.SetResearchPoint(Random.Range(200, 9999));
        isDebugModeInitialized = true;

        // �����ł���q���C����ǉ�
        PlayerCharacterDefine First = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/1.Battler");

        PlayerCharacterDefine Second = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/2.TentacleMan");

        AddHisui(true);
        AddDaiya(true);

        // �t�H�[���[�V�����Ґ�
        playerData.formationCharacters[0].characterID = First.detail.characterID;
        playerData.formationCharacters[1].characterID = Second.detail.characterID;

        // �������Ȃ��悤�ɂ�����Ƌ�������
        playerData.characters[0].current_maxHp += 1000;
        playerData.characters[0].current_hp += 1000;
        playerData.characters[1].current_maxHp += 1000;
        playerData.characters[1].current_hp += 1000;

        // �A�C�e������������������
        ItemDefine bread = Resources.Load<ItemDefine>("ItemList/�H�p��");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(bread);
        //Resources.UnloadAsset(bread);

        ItemDefine croissant = Resources.Load<ItemDefine>("ItemList/�N�����b�T��");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(croissant);
        //Resources.UnloadAsset(croissant);

        ItemDefine m24 = Resources.Load<ItemDefine>("ItemList/M24");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(m24);
        //Resources.UnloadAsset(m24);

        ItemDefine aid = Resources.Load<ItemDefine>("ItemList/�~�}��");
        for (int i = 0; i < Random.Range(2, 5); i++) playerData.inventory.Add(aid);
        //Resources.UnloadAsset(aid);

        // �S�������J������
        EquipmentDefine[] allEquipment = Resources.LoadAll<EquipmentDefine>("EquipmentList");
        foreach (EquipmentDefine equip in allEquipment)
        {
            AddNewEquipment(equip);
            //Resources.UnloadAsset(equip);
        }

        // �G�L������ݒu
        if (addEnemy)
        {
            BattleSetup.Reset(false);
            BattleSetup.AddEnemy("Hisui_Enemy_Final");
            BattleSetup.AddEnemy("Kei_Enemy_Final");
            BattleSetup.SetBattleBGM("apoptosis");
            BattleSetup.SetBattleBack(BattleBack.CentreTower);
        }

        // �`���[�g���A���S�J��
        TutorialData tutorial = new TutorialData();
        tutorial.characterbuildingPanel = true;
        tutorial.formationPanel = true;
        tutorial.trainPanel = true;
        tutorial.worldscene = true;
        SetTutorialData(tutorial);

        // DLC
        DLCManager.isDLCEnabled = true;
    }

    public void AddHisui(bool isCorrupted = false)
    {
        PlayerCharacterDefine Hisui = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/12.Hisui");
        AddPlayerCharacter(Hisui).is_corrupted = isCorrupted;
    }

    public void AddDaiya(bool isCorrupted = false)
    {
        PlayerCharacterDefine Daiya = Resources.Load<PlayerCharacterDefine>("PlayerCharacterList/13.Daiya");
        AddPlayerCharacter(Daiya).is_corrupted = isCorrupted;
    }
#else
    public void DebugModeInitialize() { }
#endif
}

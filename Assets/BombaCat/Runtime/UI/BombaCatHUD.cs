using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

/// <summary>Responsive BombaCat HUD, built from the existing gameplay components.</summary>
public sealed class BombaCatHUD : MonoBehaviour
{
    private static readonly Color Ink=new Color32(25,22,36,255), Panel=new Color32(40,34,53,247),
        Raised=new Color32(58,48,70,255), Cream=new Color32(255,242,216,255), Muted=new Color32(181,168,190,255),
        Coral=new Color32(255,115,107,255), Mint=new Color32(144,231,196,255), Gold=new Color32(255,201,104,255);
    private Canvas canvas;
    private RectTransform safeRoot,modal,bossFill,abilityDock,stealthSlot;
    private TMP_FontAsset font;
    private TMP_Text healthLabel,enemyLabel,objectiveLabel,boomLabel,stealthLabel,bossLabel,boomCountdown,stealthCountdown,pauseTitle;
    private GameObject bossRoot;
    private BombaGlyph healthGraphic,healthTrack,damageFlash,boomRing,stealthRing,boomIcon,stealthIcon;
    private CanvasGroup stealthGroup,checkpointGroup;
    private TMP_Text checkpointText;
    private Sprite catPortrait;
    private PlayerController player;
    private PlayerHealth health;
    private ExplosionController explosion;
    private PlayerStealth stealth;
    private LevelManager level;
    private float nextRefresh,nextPlayerSearch,healthTarget=1f,healthDisplay=1f,flash,stealthReveal,pauseTypingStarted;
    private int lastHealth=-1,lastMax=-1,lastEnemies=-1,lastRemaining=-1,lastBoomTenths=-1,lastStealthTenths=-1,lastStealthState=-1;
    private int lastTypingFrame=-1;
    private bool pauseMenu;
    private static readonly string[] NapFrames={"N","Na","Nap","Napp","Nappi","Nappin","Napping","Napping.","Napping..","Napping..."};
    private Rect lastSafeArea;
    private int lastWidth,lastHeight;

    public void Build(TMP_FontAsset suppliedFont)
    {
        if(canvas!=null) return;
        font=suppliedFont;
        catPortrait=Resources.Load<Sprite>("BombaCatPortrait");
        var root=new GameObject("BombaCat HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        root.transform.SetParent(transform,false);
        canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=20;
        var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1600,900); scaler.matchWidthOrHeight=.5f;
        safeRoot=MakeRect("Safe area",canvas.transform); Stretch(safeRoot); ApplySafeArea();
        BuildHealth(); BuildObjective(); BuildAbilities(); BuildCheckpointToast();
        var pause=ButtonAt(safeRoot,"ESC  /  PAUSE",new Vector2(1,1),new Vector2(-28,-320),new Vector2(154,34),Raised,OpenPause);
        pause.GetComponent<RectTransform>().pivot=new Vector2(1,1);
        var boss=MakeRect("Boss",safeRoot); Place(boss,new Vector2(.5f,1),new Vector2(0,-30),new Vector2(440,76),new Vector2(.5f,1));
        bossRoot=boss.gameObject; Tile(boss,Panel,18);
        bossLabel=Label(boss,"BOSS",17,Cream,new Vector2(22,-12),new Vector2(396,24));
        bossFill=Bar(boss,new Vector2(22,-47),new Vector2(396,9),Coral); bossRoot.SetActive(false);
        StyleDialogue();
        var feedback=MakeRect("Damage feedback",canvas.transform); Stretch(feedback);
        damageFlash=feedback.gameObject.AddComponent<BombaGlyph>(); damageFlash.radius=0;
        damageFlash.color=new Color(Coral.r,Coral.g,Coral.b,0); damageFlash.raycastTarget=false;
        modal=MakeRect("Modal",canvas.transform); Stretch(modal); modal.gameObject.SetActive(false);
        if(EventSystem.current==null)
        {
            var events=new GameObject("BombaCat EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform,false);
        }
    }
    private void BuildHealth()
    {
        var status=MakeRect("Cat status",safeRoot);
        Place(status,new Vector2(0,1),new Vector2(24,-24),new Vector2(326,174),new Vector2(0,1));
        var portrait=MakeRect("Portrait and health",status);
        Place(portrait,new Vector2(0,1),Vector2.zero,new Vector2(166,166),new Vector2(0,1));
        CircleAt(portrait,"Portrait shadow",new Color(0,0,0,.25f),new Vector2(4,-5),166);
        CircleAt(portrait,"Blue frame",new Color32(57,111,224,255),Vector2.zero,166);
        RingAt(portrait,"Gold rim",Gold,Vector2.zero,156,.055f);
        PortraitAt(portrait,new Vector2(0,-2),138);
        healthTrack=RingAt(portrait,"Health track",Raised,Vector2.zero,172,.105f);
        healthTrack.segments=5; healthTrack.gapDegrees=5;
        healthGraphic=RingAt(portrait,"Health segments",Gold,Vector2.zero,172,.105f);
        healthGraphic.segments=5; healthGraphic.gapDegrees=5;
        healthGraphic.useGradient=true; healthGraphic.endColor=Mint;
        Label(status,"BOMBA CAT",21,Cream,new Vector2(180,-38),new Vector2(150,30),FontStyles.Bold);
        var badge=MakeRect("Health count",status);
        Place(badge,new Vector2(0,1),new Vector2(180,-77),new Vector2(103,41),new Vector2(0,1));
        Tile(badge,Panel,13);
        healthLabel=Label(badge,"5 / 5",23,Cream,Vector2.zero,new Vector2(103,41),FontStyles.Bold);
        healthLabel.alignment=TextAlignmentOptions.Center;
    }
    private void BuildObjective()
    {
        var card=MakeRect("Minimap and progress",safeRoot);
        Place(card,new Vector2(1,1),new Vector2(-28,-24),new Vector2(250,280),new Vector2(1,1));
        Tile(card,Panel,26);
        var map=MakeRect("Minimap viewport",card);
        Place(map,new Vector2(0,1),new Vector2(10,-10),new Vector2(230,230),new Vector2(0,1));
        map.gameObject.AddComponent<BombaCatMinimap>().Build(map);
        enemyLabel=Label(card,"00 CLEARED",12,Cream,new Vector2(12,-251),new Vector2(116,20),FontStyles.Bold);
        enemyLabel.alignment=TextAlignmentOptions.Center;
        objectiveLabel=Label(card,"EXPLORE",12,Muted,new Vector2(130,-251),new Vector2(108,20),FontStyles.Bold);
        objectiveLabel.alignment=TextAlignmentOptions.Center;
    }
    private void BuildAbilities()
    {
        abilityDock=MakeRect("Ability dock",safeRoot);
        Place(abilityDock,new Vector2(0,0),new Vector2(28,24),new Vector2(132,168),new Vector2(0,0));
        var explodeSlot=BuildAbilitySlot("Explode","E",BombaGlyph.Kind.Bomb,Coral,
            out boomRing,out boomIcon,out boomCountdown,out boomLabel);
        Place(explodeSlot,new Vector2(0,0),Vector2.zero,new Vector2(132,168),new Vector2(0,0));
        stealthSlot=BuildAbilitySlot("Vanish","C",BombaGlyph.Kind.Paw,Mint,
            out stealthRing,out stealthIcon,out stealthCountdown,out stealthLabel);
        Place(stealthSlot,new Vector2(0,0),new Vector2(144,0),new Vector2(132,168),new Vector2(0,0));
        stealthGroup=stealthSlot.gameObject.AddComponent<CanvasGroup>();
        stealthGroup.alpha=0; stealthGroup.interactable=false; stealthGroup.blocksRaycasts=false;
        stealthSlot.gameObject.SetActive(false);
        var hints=Label(safeRoot,"W / S  FORWARD / BACK     A / D  TURN     SHIFT  RUN     SPACE  JUMP",11,Cream,Vector2.zero,new Vector2(620,22),FontStyles.Bold);
        Place(hints.rectTransform,new Vector2(.5f,0),new Vector2(0,15),new Vector2(620,22),new Vector2(.5f,0));
        hints.alignment=TextAlignmentOptions.Center;
    }

    private RectTransform BuildAbilitySlot(string title,string key,BombaGlyph.Kind kind,Color accent,
        out BombaGlyph ring,out BombaGlyph icon,out TMP_Text countdown,out TMP_Text status)
    {
        var slot=MakeRect(title+" slot",abilityDock);
        Place(slot,Vector2.zero,Vector2.zero,new Vector2(132,168),Vector2.zero);
        var circle=MakeRect(title+" circle",slot);
        Place(circle,new Vector2(.5f,1),new Vector2(0,-2),new Vector2(116,116),new Vector2(.5f,1));
        CircleAt(circle,"Shadow",new Color(0,0,0,.25f),new Vector2(3,-4),116);
        CircleAt(circle,"Background",Panel,Vector2.zero,116);
        RingAt(circle,"Cooldown track",Raised,Vector2.zero,116,.09f);
        ring=RingAt(circle,"Radial refill",accent,Vector2.zero,116,.09f);
        var iconRect=MakeRect("Ability icon",circle);
        Place(iconRect,new Vector2(.5f,.5f),new Vector2(0,4),new Vector2(52,52),new Vector2(.5f,.5f));
        icon=iconRect.gameObject.AddComponent<BombaGlyph>(); icon.kind=kind; icon.color=accent; icon.raycastTarget=false;
        countdown=Label(circle,"",28,Cream,Vector2.zero,new Vector2(104,43),FontStyles.Bold);
        Place(countdown.rectTransform,new Vector2(.5f,.5f),new Vector2(0,1),new Vector2(104,43),new Vector2(.5f,.5f));
        countdown.alignment=TextAlignmentOptions.Center;
        var keyBadge=MakeRect("Key "+key,circle);
        Place(keyBadge,new Vector2(1,0),new Vector2(-2,0),new Vector2(29,29),new Vector2(.5f,.5f));
        CircleAt(keyBadge,"Key rim",Cream,Vector2.zero,29);
        CircleAt(keyBadge,"Key background",Ink,Vector2.zero,25);
        var keyText=Label(keyBadge,key,14,Cream,Vector2.zero,new Vector2(29,29),FontStyles.Bold);
        Stretch(keyText.rectTransform); keyText.alignment=TextAlignmentOptions.Center;
        var name=Label(slot,title.ToUpperInvariant(),16,Cream,new Vector2(0,-124),new Vector2(132,24),FontStyles.Bold);
        name.alignment=TextAlignmentOptions.Center;
        status=Label(slot,"READY",10,accent,new Vector2(0,-150),new Vector2(132,18),FontStyles.Bold);
        status.alignment=TextAlignmentOptions.Center;
        return slot;
    }
    private void Update()
    {
        if(canvas==null) return;
        ApplySafeArea();
        if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if(pauseMenu) Resume();
            else if(!modal.gameObject.activeSelf && (GameManager.Instance==null || !GameManager.Instance.IsPaused)) OpenPause();
        }
        if(player==null && Time.unscaledTime>=nextPlayerSearch)
        {
            nextPlayerSearch=Time.unscaledTime+1f; player=FindAnyObjectByType<PlayerController>();
            if(player!=null) { health=player.GetComponent<PlayerHealth>(); explosion=player.GetComponent<ExplosionController>(); stealth=player.GetComponent<PlayerStealth>(); }
        }
        if(Time.unscaledTime>=nextRefresh)
        {
            nextRefresh=Time.unscaledTime+.1f;
            if(health!=null) SetHealth(health.CurrentHealth,health.MaxHealth);
            if(GameManager.Instance!=null) SetEnemyCount(GameManager.Instance.defeatedEnemies);
            if(level==null) level=FindAnyObjectByType<LevelManager>();
            if(level!=null && lastRemaining!=level.EnemiesRemaining)
            {
                lastRemaining=level.EnemiesRemaining;
                objectiveLabel.text=lastRemaining>0 ? lastRemaining.ToString("00")+" LEFT" : "AREA CLEAR";
                objectiveLabel.color=lastRemaining>0?Muted:Mint;
            }
            RefreshAbilities();
        }
        healthDisplay=Mathf.MoveTowards(healthDisplay,healthTarget,Time.unscaledDeltaTime*1.8f);
        healthGraphic.SetFill(healthDisplay);
        flash=Mathf.MoveTowards(flash,0f,Time.unscaledDeltaTime*.65f); damageFlash.color=new Color(Coral.r,Coral.g,Coral.b,flash);
        bool lowHealth=healthTarget>0f && healthTarget<=.3f;
        healthGraphic.useGradient=!lowHealth;
        healthGraphic.color=lowHealth?Color.Lerp(Gold,Coral,.75f+.25f*Mathf.Sin(Time.unscaledTime*5f)):Gold;
        AnimateAbilities();
        if(pauseMenu && pauseTitle!=null)
        {
            int frame=Mathf.Min(9,Mathf.FloorToInt((Time.unscaledTime-pauseTypingStarted)/.16f)%19);
            if(frame!=lastTypingFrame) { lastTypingFrame=frame; pauseTitle.text=NapFrames[frame]; }
        }
    }
    private void RefreshAbilities()
    {
        if(explosion!=null)
        {
            int tenths=Mathf.CeilToInt(explosion.CooldownRemaining*10f);
            if(tenths!=lastBoomTenths)
            {
                lastBoomTenths=tenths;
                boomCountdown.text=tenths<=0?"":(tenths/10f).ToString("0.0");
                boomLabel.text=tenths<=0?"READY":"RECHARGING";
                boomLabel.color=tenths<=0?Coral:Muted;
                boomIcon.color=tenths<=0?Coral:new Color(Coral.r,Coral.g,Coral.b,.16f);
                boomIcon.enabled=tenths<=0;
            }
        }
        else { boomLabel.text="UNAVAILABLE"; boomCountdown.text=""; }
        int state=stealth==null || !stealth.HasStealth?0:stealth.IsStealthed?2:stealth.IsReady?1:3;
        int remaining=state==2?Mathf.CeilToInt(stealth.RemainingDuration*10f):
            state==3?Mathf.CeilToInt(stealth.CooldownRemaining*10f):0;
        if(state!=lastStealthState || remaining!=lastStealthTenths)
        {
            lastStealthState=state; lastStealthTenths=remaining;
            stealthCountdown.text=state==2 || state==3?(remaining/10f).ToString("0.0"):"";
            stealthLabel.text=state==2?"HIDDEN":state==3?"RECHARGING":"READY";
            stealthLabel.color=state==3?Muted:Mint;
            stealthIcon.color=state==1?Mint:new Color(Mint.r,Mint.g,Mint.b,.16f);
            stealthIcon.enabled=state==1;
        }
    }
    private void AnimateAbilities()
    {
        boomRing.SetFill(explosion!=null?1f-explosion.CooldownNormalized:0f);
        bool unlocked=stealth!=null && stealth.HasStealth;
        if(unlocked && !stealthSlot.gameObject.activeSelf) stealthSlot.gameObject.SetActive(true);
        stealthReveal=Mathf.MoveTowards(stealthReveal,unlocked?1f:0f,Time.unscaledDeltaTime*2.8f);
        float ease=stealthReveal*stealthReveal*(3f-2f*stealthReveal);
        stealthGroup.alpha=ease;
        stealthSlot.anchoredPosition=new Vector2(Mathf.Lerp(58f,144f,ease),0);
        stealthSlot.localScale=Vector3.one*Mathf.Lerp(.65f,1f,ease);
        abilityDock.sizeDelta=new Vector2(Mathf.Lerp(132f,276f,ease),168);
        if(!unlocked && stealthReveal<=0f && stealthSlot.gameObject.activeSelf) stealthSlot.gameObject.SetActive(false);
        if(unlocked)
        {
            stealthRing.SetFill(stealth.IsStealthed?stealth.RemainingNormalized:
                stealth.IsReady?1f:1f-stealth.CooldownNormalized);
            stealthRing.color=stealth.IsStealthed?Gold:Mint;
        }
        float remaining=BombaCatCheckpointService.StatusUntil-Time.unscaledTime;
        float alpha=remaining>0f?Mathf.Min(1f,remaining/.5f):0f;
        checkpointGroup.alpha=Mathf.MoveTowards(checkpointGroup.alpha,alpha,Time.unscaledDeltaTime*6f);
        string message=BombaCatCheckpointService.StatusMessage;
        if(remaining>0f && checkpointText.text!=message) checkpointText.text=message;
    }
    public void SetHealth(int current,int max)
    {
        max=Mathf.Max(1,max); current=Mathf.Clamp(current,0,max);
        if(current==lastHealth && max==lastMax) return;
        if(lastHealth>=0 && current<lastHealth) flash=.2f;
        lastHealth=current; lastMax=max; healthTarget=(float)current/max;
        healthLabel.text=current+" / "+max; healthLabel.color=current>0 && healthTarget<=.3f?Gold:Cream;
        healthTrack.SetSegments(max); healthGraphic.SetSegments(max);
    }
    public void SetEnemyCount(int count) { if(lastEnemies==count) return; lastEnemies=count; enemyLabel.text=Mathf.Max(0,count).ToString("00")+" CLEARED"; }
    public void SetBossHealth(int current,int max) { bossRoot.SetActive(true); bossLabel.text="BOSS     "+Mathf.Max(0,current)+" / "+Mathf.Max(1,max); Fill(bossFill,(float)current/Mathf.Max(1,max)); }
    public void SetBossMeter(float normalized) { bossRoot.SetActive(true); Fill(bossFill,normalized); }
    public void CloseModal()
    {
        if(modal==null) return;
        for(int i=modal.childCount-1;i>=0;i--) { var child=modal.GetChild(i).gameObject; child.SetActive(false); Destroy(child); }
        modal.gameObject.SetActive(false); pauseMenu=false; pauseTitle=null;
    }
    private void BeginModal() { CloseModal(); modal.gameObject.SetActive(true); Tile(modal,new Color(Ink.r,Ink.g,Ink.b,.91f),0).raycastTarget=true; SetPaused(true); }
    private static void SetPaused(bool paused)
    {
        if(GameManager.Instance!=null) GameManager.Instance.SetPaused(paused);
        else { Time.timeScale=paused?0f:1f; Cursor.visible=paused; Cursor.lockState=paused?CursorLockMode.None:CursorLockMode.Locked; }
    }
    private void OpenPause()
    {
        if(modal.gameObject.activeSelf || (health!=null && health.IsDead)) return;
        BeginModal(); pauseMenu=true;
        var box=ModalCard(new Vector2(500,445));
        var portrait=MakeRect("Napping cat",box);
        Place(portrait,new Vector2(.5f,1),new Vector2(0,-28),new Vector2(140,140),new Vector2(.5f,1));
        CircleAt(portrait,"Blue frame",new Color32(57,111,224,255),Vector2.zero,140);
        RingAt(portrait,"Gold rim",Gold,Vector2.zero,140,.065f);
        PortraitAt(portrait,Vector2.zero,125);
        pauseTitle=CenterLabel(box,"N",43,Cream,-188,60);
        pauseTypingStarted=Time.unscaledTime; lastTypingFrame=-1;
        Focus(ButtonAt(box,"RESUME",new Vector2(.5f,1),new Vector2(0,-282),new Vector2(348,56),Mint,Resume));
        ButtonAt(box,"RESTART LEVEL",new Vector2(.5f,1),new Vector2(0,-353),new Vector2(348,48),Raised,()=>
        { if(GameManager.Instance!=null) GameManager.Instance.RestartLevel(); });
    }
    private void Resume() { CloseModal(); SetPaused(false); }
    public void ShowResult(string kicker,string title,string description,bool won,bool canContinue=false)
    {
        BeginModal(); var box=ModalCard(new Vector2(620,490));
        Icon(box,won?BombaGlyph.Kind.Paw:BombaGlyph.Kind.Cat,won?Mint:Coral,new Vector2(266,-30),new Vector2(88,88));
        CenterLabel(box,kicker,13,won?Mint:Gold,-137,25); CenterLabel(box,title,49,Cream,-172,66); CenterLabel(box,description,17,Muted,-249,35);
        int score=GameManager.Instance!=null?GameManager.Instance.defeatedEnemies:Mathf.Max(0,lastEnemies);
        CenterLabel(box,score+" FOES CLEARED",18,Cream,-303,30);
        Focus(ButtonAt(box,canContinue?"NEXT AREA":won?"PLAY AGAIN":"RETRY CHECKPOINT",new Vector2(.5f,1),new Vector2(0,-373),new Vector2(400,58),won?Mint:Coral,()=>
        {
            if(canContinue) { var current=FindAnyObjectByType<LevelManager>(); if(current!=null) { current.ContinueAfterUpgrade(); return; } }
            if(GameManager.Instance!=null) { if(won) GameManager.Instance.RestartRun(); else GameManager.Instance.RestartGame(); }
        }));
    }
    public void ShowUpgrades(UpgradeData[] choices)
    {
        if(choices==null || choices.Length==0) return;
        BeginModal();
        var title=Label(modal,"A LITTLE MORE CAT-ASTROPHE",13,Gold,Vector2.zero,new Vector2(1000,30),FontStyles.Bold);
        Place(title.rectTransform,new Vector2(.5f,.5f),new Vector2(0,268),new Vector2(1000,30),new Vector2(.5f,.5f)); title.alignment=TextAlignmentOptions.Center;
        var heading=Label(modal,"Pick your next power",43,Cream,Vector2.zero,new Vector2(1100,70),FontStyles.Bold);
        Place(heading.rectTransform,new Vector2(.5f,.5f),new Vector2(0,213),new Vector2(1100,70),new Vector2(.5f,.5f)); heading.alignment=TextAlignmentOptions.Center;
        int count=Mathf.Min(3,choices.Length); Button first=null;
        for(int i=0;i<count;i++)
        {
            var choice=choices[i]; if(choice==null) continue; int index=i;
            var card=MakeRect("Upgrade "+(i+1),modal); Place(card,new Vector2(.5f,.5f),new Vector2((i-(count-1)*.5f)*326f,-30),new Vector2(306,350),new Vector2(.5f,.5f));
            Tile(card,Panel,23); Color accent=i==0?Coral:i==1?Mint:Gold;
            var icon=choice.type==UpgradeType.Stealth || choice.type==UpgradeType.MoveSpeed?BombaGlyph.Kind.Paw:choice.type==UpgradeType.MaxHealth || choice.type==UpgradeType.Heal?BombaGlyph.Kind.Cat:BombaGlyph.Kind.Bomb;
            Icon(card,icon,accent,new Vector2(24,-25),new Vector2(60,60));
            Label(card,"UPGRADE  0"+(i+1),11,accent,new Vector2(105,-42),new Vector2(177,28),FontStyles.Bold);
            var name=Label(card,choice.upgradeName,25,Cream,new Vector2(25,-111),new Vector2(256,66),FontStyles.Bold); name.enableAutoSizing=true; name.fontSizeMin=17; name.fontSizeMax=25;
            var body=Label(card,choice.description,15,Muted,new Vector2(25,-187),new Vector2(256,78)); body.enableAutoSizing=true; body.fontSizeMin=12; body.fontSizeMax=15;
            var button=ButtonAt(card,"MAKE IT MINE",new Vector2(.5f,1),new Vector2(0,-282),new Vector2(256,46),accent,()=>
            { if(UpgradeManager.Instance==null) return; CloseModal(); UpgradeManager.Instance.ChooseUpgrade(index); });
            if(first==null) first=button;
        }
        Focus(first);
        var note=Label(modal,"Choose one. Make every life count.",14,Muted,Vector2.zero,new Vector2(1000,28));
        Place(note.rectTransform,new Vector2(.5f,.5f),new Vector2(0,-245),new Vector2(1000,28),new Vector2(.5f,.5f)); note.alignment=TextAlignmentOptions.Center;
    }
    private static void Focus(Button button) { if(button!=null && EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(button.gameObject); }
    private RectTransform ModalCard(Vector2 size) { var box=MakeRect("Card",modal); Place(box,new Vector2(.5f,.5f),Vector2.zero,size,new Vector2(.5f,.5f)); Tile(box,Panel,28); return box; }
    private TMP_Text CenterLabel(Transform parent,string text,float size,Color tint,float y,float height)
    {
        var label=Label(parent,text,size,tint,Vector2.zero,new Vector2(540,height),FontStyles.Bold);
        Place(label.rectTransform,new Vector2(.5f,1),new Vector2(0,y),new Vector2(((RectTransform)parent).sizeDelta.x-56,height),new Vector2(.5f,1)); label.alignment=TextAlignmentOptions.Center; return label;
    }
    private Button ButtonAt(Transform parent,string text,Vector2 anchor,Vector2 position,Vector2 size,Color tint,UnityAction action)
    {
        var rect=MakeRect(text,parent); Place(rect,anchor,position,size,new Vector2(.5f,1)); var graphic=Tile(rect,tint,13); graphic.raycastTarget=true;
        var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=graphic;
        var colors=button.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(1.1f,1.1f,1.1f); colors.pressedColor=new Color(.8f,.8f,.8f); colors.selectedColor=new Color(1.1f,1.1f,1.1f); colors.fadeDuration=.1f; button.colors=colors;
        button.onClick.AddListener(action);
        var label=Label(rect,text,14,tint==Raised?Cream:Ink,Vector2.zero,size,FontStyles.Bold); Stretch(label.rectTransform); label.alignment=TextAlignmentOptions.Center; return button;
    }
    private TMP_Text Label(Transform parent,string text,float size,Color tint,Vector2 position,Vector2 dimensions,FontStyles style=FontStyles.Normal)
    {
        var rect=MakeRect(text,parent); Place(rect,new Vector2(0,1),position,dimensions,new Vector2(0,1)); var label=rect.gameObject.AddComponent<TextMeshProUGUI>();
        if(font!=null) label.font=font;
        label.text=text; label.fontSize=size; label.color=tint; label.fontStyle=style; label.alignment=TextAlignmentOptions.TopLeft; label.raycastTarget=false; label.overflowMode=TextOverflowModes.Ellipsis; return label;
    }
    private void Keycap(Transform parent,string text,Vector2 position)
    {
        var key=MakeRect("Key "+text,parent); Place(key,new Vector2(0,1),position,new Vector2(30,27),new Vector2(0,1)); Tile(key,Raised,7);
        var label=Label(key,text,14,Cream,Vector2.zero,new Vector2(30,27),FontStyles.Bold); label.alignment=TextAlignmentOptions.Center;
    }
    private static void Icon(Transform parent,BombaGlyph.Kind kind,Color tint,Vector2 position,Vector2 size)
    {
        var rect=MakeRect(kind.ToString(),parent); Place(rect,new Vector2(0,1),position,size,new Vector2(0,1)); var glyph=rect.gameObject.AddComponent<BombaGlyph>(); glyph.kind=kind; glyph.color=tint; glyph.raycastTarget=false;
    }
    private static BombaGlyph CircleAt(Transform parent,string name,Color tint,Vector2 offset,float diameter)
    {
        var rect=MakeRect(name,parent);
        Place(rect,new Vector2(.5f,.5f),offset,new Vector2(diameter,diameter),new Vector2(.5f,.5f));
        var circle=rect.gameObject.AddComponent<BombaGlyph>();
        circle.kind=BombaGlyph.Kind.Circle; circle.color=tint; circle.raycastTarget=false;
        return circle;
    }
    private static BombaGlyph RingAt(Transform parent,string name,Color tint,Vector2 offset,float diameter,float thickness)
    {
        var rect=MakeRect(name,parent);
        Place(rect,new Vector2(.5f,.5f),offset,new Vector2(diameter,diameter),new Vector2(.5f,.5f));
        var ring=rect.gameObject.AddComponent<BombaGlyph>();
        ring.kind=BombaGlyph.Kind.Ring; ring.color=tint; ring.ringThickness=thickness; ring.raycastTarget=false;
        return ring;
    }
    private void PortraitAt(Transform parent,Vector2 offset,float diameter)
    {
        var maskGraphic=CircleAt(parent,"Actual cat portrait",new Color32(70,97,143,255),offset,diameter);
        var mask=maskGraphic.gameObject.AddComponent<Mask>(); mask.showMaskGraphic=true;
        var imageRect=MakeRect("Cat",maskGraphic.transform); Stretch(imageRect);
        if(catPortrait!=null)
        {
            var image=imageRect.gameObject.AddComponent<Image>();
            image.sprite=catPortrait; image.preserveAspect=true; image.raycastTarget=false;
        }
        else
        {
            // Keeps the HUD usable during import; the distributed build includes the portrait sprite.
            var fallback=imageRect.gameObject.AddComponent<BombaGlyph>();
            fallback.kind=BombaGlyph.Kind.Cat; fallback.color=Cream; fallback.raycastTarget=false;
        }
    }
    private void BuildCheckpointToast()
    {
        var toast=MakeRect("Checkpoint notice",safeRoot);
        Place(toast,new Vector2(.5f,0),new Vector2(0,53),new Vector2(460,42),new Vector2(.5f,0));
        Tile(toast,Panel,14);
        checkpointGroup=toast.gameObject.AddComponent<CanvasGroup>();
        checkpointGroup.alpha=0; checkpointGroup.interactable=false; checkpointGroup.blocksRaycasts=false;
        checkpointText=Label(toast,"",13,Mint,Vector2.zero,new Vector2(460,42),FontStyles.Bold);
        checkpointText.alignment=TextAlignmentOptions.Center;
    }
    private static RectTransform Bar(Transform parent,Vector2 position,Vector2 size,Color tint)
    {
        var track=MakeRect("Track",parent); Place(track,new Vector2(0,1),position,size,new Vector2(0,1)); Tile(track,Raised,size.y*.5f);
        var fill=MakeRect("Fill",track); Stretch(fill); Tile(fill,tint,size.y*.5f); return fill;
    }
    private static void Fill(RectTransform rect,float value) { if(rect!=null) rect.anchorMax=new Vector2(Mathf.Clamp01(value),1); }
    private static BombaGlyph Tile(Transform parent,Color tint,float radius)
    {
        var rect=MakeRect("Surface",parent); Stretch(rect); var tile=rect.gameObject.AddComponent<BombaGlyph>(); tile.color=tint; tile.radius=radius; tile.raycastTarget=false; return tile;
    }
    private static RectTransform MakeRect(string name,Transform parent)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false); return rect;
    }
    private static void Place(RectTransform rect,Vector2 anchor,Vector2 position,Vector2 size,Vector2 pivot)
    { rect.anchorMin=rect.anchorMax=anchor; rect.pivot=pivot; rect.anchoredPosition=position; rect.sizeDelta=size; }
    private static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero; }
    private void ApplySafeArea()
    {
        Rect safe=Screen.safeArea;
        if(safe==lastSafeArea && Screen.width==lastWidth && Screen.height==lastHeight) return;
        lastSafeArea=safe; lastWidth=Screen.width; lastHeight=Screen.height;
        if(lastWidth<=0 || lastHeight<=0) return;
        safeRoot.anchorMin=new Vector2(safe.xMin/lastWidth,safe.yMin/lastHeight); safeRoot.anchorMax=new Vector2(safe.xMax/lastWidth,safe.yMax/lastHeight); safeRoot.offsetMin=safeRoot.offsetMax=Vector2.zero;
    }
    private void StyleDialogue()
    {
        var dialogue=FindAnyObjectByType<DialogueUI>(); if(dialogue==null) return;
        if(dialogue.dialoguePanel!=null) dialogue.dialoguePanel.SetActive(false);
        if(dialogue.interactionPrompt!=null) dialogue.interactionPrompt.SetActive(false);
        var panel=MakeRect("Guide dialogue",safeRoot); Place(panel,new Vector2(.5f,0),new Vector2(0,190),new Vector2(420,145),new Vector2(.5f,0)); Tile(panel,Panel,22);
        Label(panel,"YOUR GUIDE",12,Gold,new Vector2(26,-18),new Vector2(368,22),FontStyles.Bold);
        dialogue.dialogueText=Label(panel,"",16,Cream,new Vector2(26,-42),new Vector2(368,70));
        Label(panel,"Q  /  CONTINUE",12,Mint,new Vector2(26,-118),new Vector2(368,22),FontStyles.Bold);
        panel.anchorMin=panel.anchorMax=new Vector2(.5f,.5f); var tail=MakeRect("Speech tail",panel); Place(tail,new Vector2(.5f,0),new Vector2(0,-5),new Vector2(22,22),new Vector2(.5f,.5f)); Tile(tail,Panel,3); tail.localRotation=Quaternion.Euler(0,0,45); dialogue.dialoguePanel=panel.gameObject; panel.gameObject.SetActive(false);
        var prompt=MakeRect("Talk prompt",safeRoot); Place(prompt,new Vector2(.5f,0),new Vector2(0,190),new Vector2(260,44),new Vector2(.5f,0)); Tile(prompt,Panel,12);
        var text=Label(prompt,"F  /  TALK TO YOUR GUIDE",12,Mint,Vector2.zero,new Vector2(260,44),FontStyles.Bold); text.alignment=TextAlignmentOptions.Center;
        dialogue.interactionPrompt=prompt.gameObject; prompt.gameObject.SetActive(false);
    }
}


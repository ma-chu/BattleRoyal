using UnityEngine;

/// <summary>
/// Движение и анимации героя
/// </summary>
 
public class HeroAnimation : MonoBehaviour
{
    [SerializeField] private float zeroZposition = 1.35f;      // позиция героя на ристалище 
    [SerializeField] private float zeroYrotation = -180f;      // вращение героя на ристалище  
    [SerializeField] private float stockXposition = -2.2f;     // начальное вращение героя 
    [SerializeField] private float startRotation = 90f;        // начальная позиция героя (позиция склада)
    [SerializeField] private float lineSpeed = 1.2f;
    [SerializeField] private float angleSpeed = 180f;
    
    [SerializeField] private Animator animator;                            
    [SerializeField] private RuntimeAnimatorController animatorControllerSwordShield;
    [SerializeField] private AnimatorOverrideController animatorController2HandedSword;
    [SerializeField] private AnimatorOverrideController animatorControllerSwordSword;
    
    private static readonly int GetHitParameter = Animator.StringToHash("GetHit");
    private static readonly int DieParameter = Animator.StringToHash("Die");
    private static readonly int AttackParameter = Animator.StringToHash("Attack");
    private static readonly int ChangeParameter = Animator.StringToHash("Change");
    
    private HeroViewManager _heroViewManager;
    private bool _isInitialized;

    // СОСТОЯНИЯ
    private bool _change;                               // Смена оружия до разворота
    private bool _toPosition;                           // Выход на ристалище - в начале боя и после смены оружия
    private bool _rotateToCenter;                       // Разворот после смены оружия

    public void Initialize(HeroViewManager heroViewManager)
    {
        _heroViewManager = heroViewManager;
        SubscribeEvents();
        animator.runtimeAnimatorController = animatorControllerSwordShield;
        animator.Rebind();
        _isInitialized = true;
    }

    // чтобы HeroAudio точно успел во время первого запуска, ибо не факт, что OnEnable HeroAnimator-а запустится раньше оного HeroAudio
    // Удалить. Инициализировать HeroAudio из HeroViewManager
    public void Start() => _heroViewManager.InvokeToPositionEvent();
    
    private void OnDisable() => UnsubscribeEvents();
    
    private void Update()
    {
        if (_change && !_heroViewManager.Dead)
            ChangeWeapon();

        if (_rotateToCenter)
            RotateToCenter();

        if (_toPosition)
            ToPosition();
    }
    
    private void SubscribeEvents()
    {
        _heroViewManager.GetHitEvent += OnHit;
        _heroViewManager.DeathEvent += OnDeath;
        _heroViewManager.AttackEvent += OnAttack;
        _heroViewManager.ChangeEvent += OnChange;
        _heroViewManager.ToPositionEvent += OnToPosition;
    }

    private void UnsubscribeEvents()
    {
        if (!_isInitialized)
            return;
        
        _heroViewManager.GetHitEvent -= OnHit;
        _heroViewManager.DeathEvent -= OnDeath;
        _heroViewManager.AttackEvent -= OnAttack;
        _heroViewManager.ChangeEvent -= OnChange;
        _heroViewManager.ToPositionEvent -= OnToPosition;
    }

    public void SetStartPosition()  
    {
        transform.position = new Vector3(stockXposition, 0, zeroZposition);
        transform.rotation = Quaternion.Euler(0f, startRotation, 0f);
    }

    private void OnHit(int strikeNumber = 0, int gotDamage = 0)
    {
        animator.SetBool(GetHitParameter, true);   
    }
    private void OnDeath()
    {
        animator.SetBool(DieParameter, true);
        _change = false;
    }
    private void OnChange()
    {
        _change = true;
    }
    
    private void OnAttack()
    {
        animator.SetBool(AttackParameter, true);                      
    }
    
    private void OnToPosition()
    {
        _toPosition = true;
    }
    
    /// <summary>
    /// Смена оружия
    /// </summary>
    private void ChangeWeapon()
    {
        if (!SmoothRotation(90f))
            return;
        
        if (!animator.GetBool(ChangeParameter))
            animator.SetBool(ChangeParameter, true);

        if (!SmoothMotion(stockXposition)) 
            return;
        
        _change = false;
        animator.SetBool(ChangeParameter, false);

        switch (_heroViewManager.WeaponSet)
        {
            case WeaponSet.SwordShield:
                _heroViewManager.SetSwordShield();
                animator.runtimeAnimatorController = animatorControllerSwordShield;
                break;
            case WeaponSet.SwordSword:
                _heroViewManager.SetSwordSword();
                animator.runtimeAnimatorController = animatorControllerSwordSword;
                break;
            case WeaponSet.TwoHandedSword:
                _heroViewManager.Set2HandedSword();
                animator.runtimeAnimatorController = animatorController2HandedSword;
                break;
        }

        _rotateToCenter = true;
    }
    
    /// <summary>
    /// Разворот после смены оружия
    /// </summary>
    private void RotateToCenter()
    {
        if (SmoothRotation(270f))
        {
            _rotateToCenter = false;
            _heroViewManager.InvokeToPositionEvent();
        }
    }

    /// <summary>
    /// Выход на центр ристалища
    /// </summary>
    private void ToPosition()
    {
        if (!SmoothMotion(0f))
            return;
        
        if (SmoothRotation(359.9f))
            _toPosition = false;     // сбрасываем глобальный триггер, когда стоим лицом к врагу                                                      
    }

    /// <summary>
    /// Функция плавного поворота до newY [0-360) градусов вокруг оси Y. Возвращает true, если поворот достигнут.
    // Если надо прокрутиться через 0, использовать 2 раза: до 359 и далее...
    // Вариант со штатным демпфером - наверное, он хорош, когда мы не знаем угла (и скорости) поворота заранее, а сейчас слишком сложен
    // private float RotationVelocity;                                             // переменная, нужная Mathf.SmoothDampAngle. Задать глобально
    // float yAngel = Mathf.SmoothDampAngle(0f, 90f, ref RotationVelocity, 0.3f);  // Рассчитать угол, на который надо повернуться за такт, чтоб на 90 градусов повернуться за 0.3 сек
    // Quaternion rotationToStore = Quaternion.Euler(0f, yAngel, 0f);              // Выдать вращение, равное этому углу, относительно оси Y в кватернионе
    // transform.rotation *= rotationToStore;                                      // Применить его к текущему вращению
    /// </summary>
    /// <param name="newYRotation"></param>
    /// <returns></returns>
    private bool SmoothRotation(float newYRotation)
    {
        var newAbsYRotation = newYRotation + zeroYrotation;
        var currentYRotation = _heroViewManager.transform.rotation.eulerAngles.y;
        if (currentYRotation.Equals(newAbsYRotation))
            return true;

        Quaternion deltaRotation = Quaternion.Euler(0f, angleSpeed * Time.deltaTime, 0f);
        if (currentYRotation + deltaRotation.eulerAngles.y >= 360f)               // на этом шаге перешагнем через 0... Все остальное вращение произойдет мигом
            currentYRotation = newAbsYRotation;
        
        _heroViewManager.transform.rotation *= deltaRotation;                     // крутимся по часовой
        //transform.rotation *= Quaternion.Inverse(newRotation);                  // так было бы против часовой...
        if (!(currentYRotation >= newAbsYRotation)) 
            return false;
        
        _heroViewManager.transform.rotation = Quaternion.Euler(0f, newAbsYRotation + 0.1f, 0f);     // подравнять вращение
        return true;
    }

    /// <summary>
    /// Функция плавного перемещения вдоль оси X. Возвращает true, если нужная позиция достигнута
    ///Вариант со штатным демпфером. Наверное, он хорош, когда мы не знаем вектора (и скорости) заранее, а сейчас слишком сложен
    ///private Vector3 Velocity = Vector3.zero;                             // эту переменную надо задать глобально
    ///Vector3 Destination = new Vector3(2.5f, 0, -1.5f);                   // вектор края ристалища (туда бежим на смену оружия)
    ///transform.position = Vector3.SmoothDamp(transform.position, Destination, ref Velocity, 0.6f);
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    
    private bool SmoothMotion(float x)
    {
        if (_heroViewManager.transform.position.x.Equals(x)) 
            return true;                                                     // перемещение достигнуто
       
        if (x > _heroViewManager.transform.position.x)                                                           // Если координату X необходимо увеличивать
        {
            var destination = new Vector3(x + 0.1f, 0, zeroZposition) - _heroViewManager.transform.position; // то увеличиваем
            _heroViewManager.transform.position += destination * lineSpeed * Time.deltaTime;
            if (_heroViewManager.transform.position.x >= x)                                                      // и сравниваем, не стало ли X больше задания
            {
                _heroViewManager.transform.position = new Vector3(x, 0, zeroZposition);                          // подравнять X
                return true;
            }
            
            return false;
        }
        else                                                                                                // иначе координату X необходимо уменьшать
        {
            var destination = new Vector3(x - 0.1f, 0, zeroZposition) - _heroViewManager.transform.position; // уменьшаем
            _heroViewManager.transform.position += destination * lineSpeed * Time.deltaTime;
            if (_heroViewManager.transform.position.x <= x)                                                      // и сравниваем, не стало ли X меньше задания
            {
                _heroViewManager.transform.position = new Vector3(x, 0, zeroZposition);                          // подравнять X
                return true;
            }
            
            return false;
        }
    }
}

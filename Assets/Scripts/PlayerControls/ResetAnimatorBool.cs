using UnityEngine;

public class ResetAnimatorBool : StateMachineBehaviour
{
    public string targetBool;
    public bool status;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Girdiğimizde de resetlemek isteyebiliriz ama genelde "isInteracting" Player içindir.
        animator.SetBool("isInteracting", false);
        
        // Eğer özel bir bool belirttiysek onu resetle
        if(!string.IsNullOrEmpty(targetBool))
        {
            animator.SetBool(targetBool, status);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animasyon bittiğinde (Exit) de çalışsın
        if(!string.IsNullOrEmpty(targetBool))
        {
            animator.SetBool(targetBool, status);
        }
    }
}

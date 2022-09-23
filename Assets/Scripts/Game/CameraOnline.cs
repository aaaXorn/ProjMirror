using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOnline : MonoBehaviour
{
	[HideInInspector]
    //referência global da instância
	//é igual pra todas as instâncias do CameraOnline
	//pode ser acessado por qualquer script usando CameraOnline.Instance
    public static CameraOnline Instance { get; private set; }
	
	//alvo da câmera
	private Transform Target;

	[SerializeField] Vector3 Offset;

	float smoothTime = 0.3f;
	Vector3 velocity = Vector3.zero;

	//muda o alvo da câmera
	//chamado no OnStartAuthority() do script do player
	//já está definindo o alvo como o jogador local
	//ou seja, se tiver 2 jogadores, cada um vai ver a câmera mirando no seu personagem
	public void ChangeTarget(Transform transf)
	{
		Target = transf;
	}
	
	private void Awake()
    {
        //define a instância, para outros scripts poderem
		//acessá-la sem precisar do GetComponent
        if (Instance == null) Instance = this;
        //se já tem uma instância, 
        else
		{
			//destrói a antiga
			Destroy(Instance);
			//define a nova instância
			Instance = this;
		}
    }
	
	void FixedUpdate()
	{
		if(Target != null) transform.position = Vector3.SmoothDamp(transform.position, Target.position + Offset, ref velocity, smoothTime);
	}
}

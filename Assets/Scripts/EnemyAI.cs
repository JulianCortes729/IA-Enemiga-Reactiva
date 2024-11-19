using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class EnemyAI : MonoBehaviour
{

    [SerializeField]
    private Transform player; //referencia al player

    [SerializeField]
    private float detectionRange = 10f; //ditancia maxima de deteccion

    [SerializeField]
    private float fieldOfView = 60f; //angulo de vision del enemigo

    [SerializeField]
    private LayerMask obstructionMask; //capas que pueden bloquear la vision del enemigo (layer = obstacles, defuault)

    [SerializeField]
    private LayerMask playerMask; //capa que identifica al jugador (layer = player)

    private NavMeshAgent agent;//refencia al NavMeshAgent (Controla el movimiento del enemigo en el NavMesh)
    private EnemyState currentState = EnemyState.PATROL;

    [SerializeField]
    private Transform[] patrolPoints; // Puntos de patrullaje
    private int currentPatrolIndex = 0;



    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();
    }

    void Update()
    {
        switch (currentState){
            case EnemyState.PATROL:
                PatrolBehavior();
                break;
            case EnemyState.CHASE:
                ChaseBehavior();
                break;
            case EnemyState.SEARCH:
                break;
        }
    }


    void GoToNextPatrolPoint(){
        if(patrolPoints.Length == 0) return;

        //establecer el destino.
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        //Incrementa el índice para pasar al siguiente punto
        //Usa % para que el índice vuelva al inicio
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void PatrolBehavior(){
        //Comprueba si el agente no tiene un camino pendiente
        //y si está cerca del destino
        if(!agent.pathPending && agent.remainingDistance < 0.5f){
            GoToNextPatrolPoint();
        }

        //para detectar al jugador mientras patrulla.
        CheckForPlayer();
    }

    void CheckForPlayer()
    {
        //Usa la posición del jugador y la del enemigo para calcular un vector de dirección.
        Vector3 directionToPlayer = (player.position - transform.position).normalized;  

        //Determina la distancia entre el enemigo y el jugador.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //Compara la distancia calculada con el rango de detección
        if(distanceToPlayer < detectionRange){

            //Calcula el ángulo entre la dirección frontal del enemigo (transform.forward) y la dirección hacia el jugador.
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            //Compara con la mitad del campo de visión.
            if(angleToPlayer <= fieldOfView / 2){

                //Lanza un Raycast hacia el jugador para asegurarse de que no haya obstáculos en la línea de visión. 
                //Si el Raycast no golpea un obstáculo (!Physics.Raycast), el enemigo detecta al jugador.
                if(!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask)){
                    ChangeState(EnemyState.CHASE);
                }
            }
        }

    }


    void ChaseBehavior(){

        //para actualizar continuamente la posición del jugador como destino.
        agent.SetDestination(player.position);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        
        //pierde al jugador si La distancia es mayor al rango de detección o hay un obstaculo en la linea de vision
        if(distanceToPlayer > detectionRange || Physics.Raycast(transform.position, directionToPlayer,distanceToPlayer, obstructionMask)){

            ChangeState(EnemyState.SEARCH);

        }

    }

    //Cambia currentState al nuevo estado.
    void ChangeState(EnemyState newState){
        currentState = newState;

        switch(newState){

            case EnemyState.PATROL:
                agent.isStopped = false;
                GoToNextPatrolPoint();
                break;
            
            case EnemyState.CHASE:
                agent.isStopped = false;
                break;

            case EnemyState.SEARCH:
                agent.isStopped = true;
                break;
        }

    }
}

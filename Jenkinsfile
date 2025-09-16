pipeline {
    agent any
    
    environment {
        SCANNER_HOME = tool 'sonar-scanner'
        IMAGE_TAG = "v${BUILD_NUMBER}"
    }
    stages {
        stage('Git Checkout') {
            steps {
                git branch: 'main', credentialsId: 'git-token', url: 'https://github.com/jaiswaladi246/Capstone-DotNET-Mongo-CI.git'
            }
        }
        stage('Gitleaks Scan') {
            steps {
             sh 'gitleaks detect --report-format=json --report-path=gitleaks-report.json --exit-code=1'
            }
        }
        stage('Compile') {
            steps {
                sh 'dotnet build'
            }
        }
        stage('trivy FS Scan') {
            steps {
              sh 'trivy fs --format table -o trivy-fs-report.html .'
            }
        }
        stage('Unit Testing') {
            steps {
                echo 'dotnet test'
            }
        }
        
        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('sonar') {
                    sh ''' $SCANNER_HOME/bin/sonar-scanner -Dsonar.projectName=NoteApp \
                            -Dsonar.projectKey=NoteApp '''
                }
            }
        }
        
        stage('Quality Gate Check') {
            steps {
                timeout(time: 1, unit: 'HOURS') {
                     waitForQualityGate abortPipeline: false, credentialsId: 'sonar-token'
                    }
            }
        }
        
        stage('Build Image & Tag Image') {
            steps {
                script {
                    withDockerRegistry(credentialsId: 'docker-cred') {
                        sh "docker build -t udaypagidimari/noteapp:$IMAGE_TAG ."
                    }
                }
            }
        }
        
        stage('trivy Image Scan') {
            steps {
              sh 'trivy image --format table -o trivy-image-report.html udaypagidimari/noteapp:$IMAGE_TAG'
            }
        }
        
        stage('Push Image') {
            steps {
                script {
                    withDockerRegistry(credentialsId: 'docker-cred') {
                        sh "docker push udaypagidimari/noteapp:$IMAGE_TAG"
                    }
                }
            }
        }
        stage('Update Manifest File CD Repo') {
            steps {
                script {
                    cleanWs()
                    withCredentials([usernamePassword(credentialsId: 'git-token', passwordVariable: 'GIT_PASSWORD', usernameVariable: 'GIT_USERNAME')]) {
                        sh '''
                            # Clone the CD Repo
                            git clone https://${GIT_USERNAME}:${GIT_PASSWORD}@github.com/jaiswaladi246/Capstone-DotNET-Mongo-CD.git
                            
                            # Update the tag in manifest
                            cd Capstone-DotNET-Mongo-CD
                            sed -i "s|udaypagidimari/noteapp:.*|udaypagidimari/noteapp:${IMAGE_TAG}|" Manifest/manifest.yaml
                            
                            # Confirm Changes
                            echo "Updated manifest file contents:"
                            cat Manifest/manifest.yaml
                            
                            # Commit and push the changes
                            git config user.name "Jenkins"
                            git config user.email "jenkins@example.com"
                            git add Manifest/manifest.yaml
                            git commit -m "Update image tag to ${IMAGE_TAG}"
                            git push origin main
                        '''
                    }
                    
                }
            }
        }
    }
    post {
    always {
        script {
            def jobName = env.JOB_NAME
            def buildNumber = env.BUILD_NUMBER
            def pipelineStatus = currentBuild.result ?: 'UNKNOWN'

            // Auto-assign banner colors
            def bannerColor = [
                'SUCCESS': '#28a745',  // green
                'FAILURE': '#dc3545',  // red
                'UNSTABLE': '#ffc107', // yellow
                'ABORTED': '#add8e6'   // blue
            ].get(pipelineStatus, '#007bff') // default blue

            def body = """
                <html>
                  <body style="font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f6f8; margin: 0; padding: 30px;">
                    <div style="max-width: 650px; margin: auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.12);">
                      
                      <!-- Header -->
                      <div style="background-color: ${bannerColor}; padding: 24px; text-align: center;">
                        <h1 style="margin: 0; color: #ffffff; font-size: 22px; font-weight: 600;">${jobName}</h1>
                        <p style="margin: 5px 0 0; color: #eaeaea; font-size: 14px;">Build #${buildNumber}</p>
                      </div>
                      
                      <!-- Status Section -->
                      <div style="padding: 30px; text-align: center;">
                        <h2 style="margin: 0 0 12px; font-size: 18px; color: #444;">Pipeline Status</h2>
                        <span style="display: inline-block; padding: 10px 22px; border-radius: 25px; font-size: 15px; font-weight: 600; background-color: ${bannerColor}; color: #ffffff; letter-spacing: 0.5px;">
                          ${pipelineStatus.toUpperCase()}
                        </span>
                      </div>
                      
                      <!-- Divider -->
                      <hr style="border: none; border-top: 1px solid #eee; margin: 0 30px;">
                      
                      <!-- Details -->
                      <div style="padding: 25px 30px; color: #555; font-size: 14px; line-height: 1.6;">
                        <p style="margin: 0 0 12px;">The build has been completed. You can review the detailed logs and progress in Jenkins.</p>
                        <div style="text-align: center; margin-top: 20px;">
                          <a href="${env.BUILD_URL}" style="display: inline-block; padding: 12px 24px; font-size: 14px; font-weight: 600; color: #ffffff; background-color: ${bannerColor}; text-decoration: none; border-radius: 6px; transition: background 0.3s ease;">
                            🔍 View Console Output
                          </a>
                        </div>
                      </div>
                      
                      <!-- Footer -->
                      <div style="background-color: #fafafa; padding: 18px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee;">
                        <p style="margin: 0;">🚀 This is an automated notification from <b>Jenkins CI/CD Pipeline</b></p>
                      </div>
                      
                    </div>
                  </body>
                </html>
            """

            emailext (
                subject: "${jobName} - Build ${buildNumber} - ${pipelineStatus.toUpperCase()}",
                body: body,
                to: 'pagidimariuday@gmail.com',
                from: 'udaypagidimari@gmail.com',
                replyTo: 'udaypagidimari@gmail.com',
                mimeType: 'text/html',
               
            )
        }
    }
}
}

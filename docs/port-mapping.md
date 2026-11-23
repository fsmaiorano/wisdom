| Services    | Local Env   | Docker Env  | Docker Inside |
|-------------|-------------|-------------|---------------|
| Bff         | 5001 - 5051 | 6100 - 6150 | 8080          |
| UserApi     | 5006 - 5066 | 6006 - 6066 | 8080          |
| CourseApi   | 5007 - 5077 | 6007 - 6077 | 8080          |
| MediaApi    | 5008 - 5088 | 6008 - 6088 | 8080          |
| OrderApi    | 5009 - 5099 | 6009 - 6099 | 8080          |
| LearningApi | 5010 - 5100 | 6010 - 6100 | 8080          |

| Others     | Local Env | Docker Env | Docker Inside |
|------------|-----------|------------|---------------|
| KeycloakDb | 5400      | 5400       | 5432          |
| UserDb     | 5401      | 5401       | 5432          |
| CourseDb   | 5402      | 5402       | 5432          |
| LearningDb | 5403      | 5403       | 5432          |
| MediaDb    | 5404      | 5404       | 5432          |
| OrderDb    | 5405      | 5405       | 5432          |
| Streaming  | 5011      | 6011       | 8080          |
| MediaWorker (health) | 5012 | 6012    | 8080          |
| Seq UI/API | 5500      | 5500       | 80            |

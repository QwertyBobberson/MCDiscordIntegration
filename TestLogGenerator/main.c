#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
#include <time.h>
#include <pthread.h>
#include <unistd.h>

int GetIndexOf(char*, char*);
void SplitString(char*, char*, char**, char**);
void *GetInput(void*);

#define PrintVal(val) printf("%s: %d\n", #val, val);

bool quit = false;

int main(void*)
{
    time_t t;
    srand((unsigned) time(&t));
    struct tm * timeInfo;

    char* usernames[] = {"QwertyBobberson", "BobBobbersonThe3", "mariopuppy1", "DingusDungusJr", "Geogeo357", "Censa__"};

    char* joinString = "[JOIN] [A-z]+ joined the game";

    char* leaveString = "[LEAVE] [A-z]+ left the game";

    char* usernamePattern = "[A-z]+";

    char* joinStart;
    char* joinEnd;

    char timeString[20];
    SplitString(joinString, usernamePattern, &joinStart, &joinEnd);

    char* leaveStart;
    char* leaveEnd;

    SplitString(leaveString, usernamePattern, &leaveStart, &leaveEnd);

    pthread_t inputThread;
    pthread_create(&inputThread, NULL, *GetInput, NULL);

    FILE *testLogs;

    while(!quit)
    {
        sleep(1);

        int user = rand() % 6;
        int action = rand() % 2;

        time(&t);
        struct tm * timeDetails = localtime(&t);
        strftime(timeString, sizeof(timeString), "%F %T", timeDetails);
        testLogs = fopen("../testLogs", "a");

        if(action == 0)
        {
            fprintf(testLogs, "%s%s%s%s\n", timeString, joinStart, usernames[user], joinEnd);
            printf("%s%s%s%s\n", timeString, joinStart, usernames[user], joinEnd);
        }
        else
        {
            fprintf(testLogs, "%s%s%s%s\n", timeString, leaveStart, usernames[user], leaveEnd);
            printf("%s%s%s%s\n", timeString, leaveStart, usernames[user], leaveEnd);
        }

        fclose(testLogs);
    }

    free(joinStart);
    free(joinEnd);
    free(leaveStart);
    free(leaveEnd);
}

void SplitString(char* string, char* splitPattern, char** start, char** end)
{
    int patternLen = strlen(splitPattern);
    int stringLen = strlen(string);

    int patternIndex = GetIndexOf(string, splitPattern);

    *start = (char*)malloc(patternIndex);
    memcpy(*start, string, patternIndex);

    int endLength = stringLen - patternIndex - patternLen;

    *end = (char*)malloc(endLength);
    memcpy(*end, string + patternIndex + patternLen, endLength);
}

int GetIndexOf(char* string, char* pattern)
{
    int stringLength = strlen(string);
    int patternLength = strlen(pattern);
    for(int i = 0; i < stringLength - patternLength; i++)
    {
        bool found = true;
        for(int j = 0; j < patternLength; j++)
        {
            if(string[i + j] != pattern[j])
            {
                found = false;
                break;
            }
        }

        if(found)
        {
            return i;
        }
    }

    return -1;
}

void *GetInput(void*)
{
    while(!quit)
    {
        char input = getchar();
        if(input == 'q' || input == 'Q')
        {
            quit = true;
        }
    }
}
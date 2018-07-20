function bError= fnInitializeSerialPortforArduino()
global g_strctDAQParams

bError = true;

lineTerminator = 10;
baudRate = 115200;
InputBufferSize = 100000; % Should be more than enough....
joker='';    
specialSettings='';
readTimeout = 5; % 5 sec?
portSettings = sprintf('%s %s BaudRate=%i InputBufferSize=%i Terminator=%i ReceiveTimeout=%f', joker, specialSettings, baudRate, InputBufferSize, lineTerminator, readTimeout );
fprintf(portSettings);
% DAMN THIS DEBUGGING
g_strctDAQParams.m_strAcqusitionCardBoard = 'COM3';



try
    g_strctDAQParams.m_hArduino = IOPort('OpenSerialPort', g_strctDAQParams.m_strAcqusitionCardBoard, portSettings);
    % g_strctDAQParams.m_hArduino = IOPort('OpenSerialPort','COM3');
catch
    fprintf('Error opening serial port!\n');
    return;
end
WaitSecs(3.3); % allow time to load the sketch?

% Start asynchronous background data collection and timestamping. Use
% blocking mode for reading data -- easier on the system:
asyncSetup = sprintf(' StartBackgroundRead=%i',  1);
IOPort('ConfigureSerialPort', g_strctDAQParams.m_hArduino, asyncSetup);

IOPort('Write',  g_strctDAQParams.m_hArduino , [10,'gettimestamp',10]);
WaitSecs(0.1);
% Really empty? Hope so.
NumBytesAvail =  IOPort('BytesAvailable', g_strctDAQParams.m_hArduino);

fprintf('\n\n\n');
fprintf(sprintf('number of bytes available: %d\n', NumBytesAvail));

if NumBytesAvail < 9
    fprintf('Failed to get a response from Arduino. Are you sure the correct Sketch is loaded?\n');
    fnCloseSerialPortforArduino();
    return;
end

strBuffer=IOPort('Read',g_strctDAQParams.m_hArduino,0,NumBytesAvail);

fprintf((char(strBuffer)));
    
if ~strcmpi(char(strBuffer(1:9)),'Timestamp')
        fprintf('Failed to get a response from Arduino. Are you sure the correct Sketch is loaded?\n');
        fnCloseSerialPortforArduino();
    return;
end

bError=false;
return;

